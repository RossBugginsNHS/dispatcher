using Octokit;
using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.WorkflowRun;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>
/// Processes web hooks from github app installations.
/// </summary>
public partial class WebhookEventProcessor
{
  public static readonly IDeserializer Deserialiser = new DeserializerBuilder()
    .WithNamingConvention(UnderscoredNamingConvention.Instance)
    .Build();

  /// <summary>
  /// Main entry point for GitHub web hooks from app installations.
  /// </summary>
  /// <param name="headers"></param>
  /// <param name="workflowRunEvent"></param>
  /// <param name="action"></param>
  /// <returns></returns>
  protected override async Task ProcessWorkflowRunWebhookAsync(
    WebhookHeaders headers,
    WorkflowRunEvent workflowRunEvent,
    WorkflowRunAction action)
  {
    await ((string)action switch
    {
      WorkflowRunActionValue.Completed => HandleWorkFlowRunCompleted(workflowRunEvent),
      _ => Task.CompletedTask
    });
  }

  private async Task HandleWorkFlowRunCompleted(WorkflowRunEvent workflowRunEvent)
  {
    LogGotWebHookCall(workflowRunEvent);
    var installClient = await GetInstallationClient(workflowRunEvent.Installation.Id);
    var sourceTriggerData = await TriggersList(workflowRunEvent, installClient);
    await Parallel.ForEachAsync(sourceTriggerData.Triggers, async (trigger, cancellation) =>
    {
      await TriggerDestinations(trigger, workflowRunEvent, installClient);
    });
  }

  /// <summary>
  /// Based on the installation provided in the webhook, create an installation
  /// client for it.
  /// </summary>
  /// <param name="installationId">Client for the installation that is being processed</param>
  /// <returns></returns>
  private async Task<GitHubClient> GetInstallationClient(long installationId)
  {
    var appClient = cs.GetAppClient();
    var install = await appClient.GitHubApps.GetInstallationForCurrent(installationId);
    var installClient = await cs.GetInstallationClient(appClient, install.Id);
    return installClient;
  }

  /// <summary>
  /// Looks for dispatches.yml in the triggering repository and reads its contents.
  /// </summary>
  /// <param name="workflowRunEvent"></param>
  /// <param name="installClient"></param>
  /// <returns></returns>
  private async Task<TriggersList> TriggersList(WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
  {
    var file = (await installClient.Repository.Content.GetAllContents(
            workflowRunEvent.Repository.Owner.Login,
            workflowRunEvent.Repository.Name,
            "dispatching.yml")).First();
    return Deserialiser.Deserialize<TriggersList>(file.Content);
  }

  /// <summary>
  /// High performance source generate logger.
  /// Do others like this and move to utility class.
  /// </summary>
  /// <param name="logger"></param>
  /// <param name="action"></param>
  /// <param name="owner"></param>
  /// <param name="repo"></param>
  /// <param name="workflow"></param>
  [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Got a webhook call of {Action} from {Owner}/{Repo}: {Workflow}")]
  protected static partial void LogGotWebHookCall(
    ILogger<WebhookEventProcessor> logger,
    string action,
    string owner,
    string repo,
    string workflow);

  /// <summary>
  /// Use HP logger with current Logger context.
  /// </summary>
  /// <param name="workflowRunEvent"></param>
  private void LogGotWebHookCall(WorkflowRunEvent workflowRunEvent) =>
    LogGotWebHookCall(
      Logger,
      workflowRunEvent.Action,
      workflowRunEvent.Repository.Owner.Login,
      workflowRunEvent.Repository.Name,
      workflowRunEvent.Workflow.Name);

  /// <summary>
  /// Trigger any valid destination workflows that are defined.
  /// </summary>
  /// <param name="trigger"></param>
  /// <param name="workflowRunEvent"></param>
  /// <param name="installClient"></param>
  /// <returns></returns>
  private async Task TriggerDestinations(Trigger trigger, WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
  {
    Logger.LogInformation("Looking at trigger {Source}", trigger.Source);
    var filtered = trigger.Targets.Where(x => workflowRunEvent.Workflow.Path.EndsWith(trigger.Source));
    await Parallel.ForEachAsync(filtered, async (target, cancellation) =>
    {
      await TriggerTarget(target, workflowRunEvent, installClient);
    });

  }

  /// <summary>
  /// Trigger a sepcific target.
  /// </summary>
  /// <param name="target"></param>
  /// <param name="workflowRunEvent"></param>
  /// <param name="installClient"></param>
  /// <returns></returns>
  private async Task TriggerTarget(Target target, WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
  {
    var dispatchTask = CreateDispatch(target, workflowRunEvent, installClient);
    var issueTask = CreateIssue(target, workflowRunEvent, installClient);
    await Task.WhenAll(dispatchTask, issueTask);
    Logger.LogInformation("Created Issue {Issue}", issueTask.Result.Id);
  }

  /// <summary>
  /// Trigger the given workflow dispatch
  /// </summary>
  /// <param name="target"></param>
  /// <param name="workflowRunEvent"></param>
  /// <param name="installClient"></param>
  /// <returns></returns>
  private async Task CreateDispatch(Target target, WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
  {
    await installClient.Actions.Workflows.CreateDispatch(
      workflowRunEvent.Repository.Owner.Login,
      target.Repository,
      target.Workflow, new CreateWorkflowDispatch("main"));
  }

  /// <summary>
  /// Create a notification issue in the target repo.
  /// </summary>
  /// <param name="target"></param>
  /// <param name="workflowRunEvent"></param>
  /// <param name="installClient"></param>
  /// <returns></returns>
  private async Task<Issue> CreateIssue(Target target, WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
  {
    var title = $"Triggered a dispatch of {target.Repository} {target.Workflow}";
    return await installClient.Issue.Create(
      workflowRunEvent.Repository.Owner.Login,
      workflowRunEvent.Repository.Name,
      new NewIssue(title) { Body = "Triggering" });
  }
}

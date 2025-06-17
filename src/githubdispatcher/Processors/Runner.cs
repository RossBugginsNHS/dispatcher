using Octokit;
using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public partial class Runner(ILogger<Runner> Logger, ClientSetup cs)
{
    private const string TriggerFileName = "dispatching.yml";

    public static readonly IDeserializer Deserialiser = new DeserializerBuilder()
.WithNamingConvention(UnderscoredNamingConvention.Instance)
.Build();

  public async Task HandleWorkFlowRunCompleted(WorkflowRunEvent workflowRunEvent)
  {
    LogGotWebHookCall(workflowRunEvent);
    var installClient = await GetInstallationClient(workflowRunEvent.Installation.Id);
    var sourceTriggerData = await GetSourceTriggersList(workflowRunEvent, installClient);
    await Parallel.ForEachAsync(sourceTriggerData.Triggers, async (trigger, cancellation) =>
    {
      await TriggerDestinations(trigger, workflowRunEvent, installClient);
    });
  }


    private async Task<GitHubClient> GetInstallationClient(long installationId)
    {
      var appClient = cs.GetAppClient();
      var install = await appClient.GitHubApps.GetInstallationForCurrent(installationId);
      var installClient = await cs.GetInstallationClient(appClient, install.Id);
      return installClient;
    }


    private async Task<TriggersList> GetSourceTriggersList(WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
    {
      var file = (await installClient.Repository.Content.GetAllContents(
              workflowRunEvent.Repository.Owner.Login,
              workflowRunEvent.Repository.Name,
              TriggerFileName)).First();
      return Deserialiser.Deserialize<TriggersList>(file.Content);
    }


    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Got a webhook call of {Action} from {Owner}/{Repo}: {Workflow}")]
    protected static partial void LogGotWebHookCall(
      ILogger<Runner> logger,
      string action,
      string owner,
      string repo,
      string workflow);
    private void LogGotWebHookCall(WorkflowRunEvent workflowRunEvent) =>
      LogGotWebHookCall(
        Logger,
        workflowRunEvent.Action,
        workflowRunEvent.Repository.Owner.Login,
        workflowRunEvent.Repository.Name,
        workflowRunEvent.Workflow.Name);


    private async Task TriggerDestinations(Trigger trigger, WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
    {
      Logger.LogInformation("Looking at trigger {Source}", trigger.Source);
      var filtered = trigger.Targets.Where(x => workflowRunEvent.Workflow.Path.EndsWith(trigger.Source));
      await Parallel.ForEachAsync(filtered, async (target, cancellation) =>
      {
        await TriggerTarget(target, workflowRunEvent, installClient);
      });

    }


    private async Task TriggerTarget(Target target, WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
    {
      var dispatchTask = CreateDispatch(target, workflowRunEvent, installClient);
      var issueTask = CreateIssue(target, workflowRunEvent, installClient);
      await Task.WhenAll(dispatchTask, issueTask);
      Logger.LogInformation("Created Issue {Issue}", issueTask.Result.Id);
    }


    private async Task CreateDispatch(Target target, WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
    {
      await installClient.Actions.Workflows.CreateDispatch(
        workflowRunEvent.Repository.Owner.Login,
        target.Repository,
        target.Workflow, new CreateWorkflowDispatch("main"));
    }


    private async Task<Issue> CreateIssue(Target target, WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
    {
      var title = $"Triggered a dispatch of {target.Repository} {target.Workflow}";
      return await installClient.Issue.Create(
        workflowRunEvent.Repository.Owner.Login,
        workflowRunEvent.Repository.Name,
        new NewIssue(title) { Body = "Triggering" });
    }
  }

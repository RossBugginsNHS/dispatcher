using System.Collections.Concurrent;
using Octokit;
using Octokit.Webhooks.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class Triggering(ILogger<Triggering> Logger, ClientSetup cs, Dispatches Dispatches, Issues Issues)
{
  private const string TriggerFileName = "dispatching.yml";
  public static readonly IDeserializer Deserialiser = new DeserializerBuilder()
.WithNamingConvention(UnderscoredNamingConvention.Instance)
.Build();


  public async Task TriggerAll(WorkflowRunEvent workflowRunEvent)
  {
    ArgumentNullException.ThrowIfNull(workflowRunEvent, nameof(workflowRunEvent));
    ArgumentNullException.ThrowIfNull(workflowRunEvent.Repository, nameof(workflowRunEvent.Repository));
    ArgumentNullException.ThrowIfNull(workflowRunEvent.Installation, nameof(workflowRunEvent.Installation));

    Logger.LogInformation("Triggering for workflow run {WorkflowRunId} in {Owner}/{Repo}",
      workflowRunEvent.WorkflowRun.Id,
      workflowRunEvent.Repository.Owner.Login,
      workflowRunEvent.Repository.Name);

    var installClient = await cs.GetInstallationClient(workflowRunEvent.Installation.Id);
    var sourceTriggerData = await GetSourceTriggersList(workflowRunEvent, installClient);
    await ParallelTriggerAll(sourceTriggerData.Outbound, installClient, workflowRunEvent);

  }

  private async Task ParallelTriggerAll(IEnumerable<Trigger> triggers, GitHubClient installClient, WorkflowRunEvent workflowRunEvent)
  {
    ArgumentNullException.ThrowIfNull(triggers, nameof(triggers));
    ArgumentNullException.ThrowIfNull(installClient, nameof(installClient));
    ArgumentNullException.ThrowIfNull(workflowRunEvent, nameof(workflowRunEvent));
    ArgumentNullException.ThrowIfNull(workflowRunEvent.Repository, nameof(workflowRunEvent.Repository));

    var bag = new ConcurrentBag<RepositoryWorkflow>();

    await Parallel.ForEachAsync(triggers, async (trigger, cancellation) =>
    {
      Logger.LogInformation("Processing trigger for source {Source}", trigger.Source);
      await TriggerDestinations(trigger, workflowRunEvent, installClient);
      bag.Add(trigger.Source);
      Logger.LogInformation("Triggered destinations for source {Source}", trigger.Source);
    });

    Logger.LogInformation("All {triggerCount} triggers processed for workflow run {WorkflowRunId} in {Owner}/{Repo}",
      bag.Count,
      workflowRunEvent.WorkflowRun.Id,
      workflowRunEvent.Repository.Owner.Login,
      workflowRunEvent.Repository.Name);

  }

  private async Task<TriggersList> GetSourceTriggersList(WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
  {
    ArgumentNullException.ThrowIfNull(workflowRunEvent, nameof(workflowRunEvent));
    ArgumentNullException.ThrowIfNull(workflowRunEvent.Repository, nameof(workflowRunEvent.Repository));
    return await GetTriggersLists(installClient, workflowRunEvent.Repository.Owner.Login, workflowRunEvent.Repository.Name);
  }

  private async Task<TriggersList> GetTriggersLists(GitHubClient installClient, string account, string repo)
  {
    try
    {
      var file = (await installClient.Repository.Content.GetAllContents(
          account,
          repo,
          TriggerFileName)).First();

      Logger.LogInformation("Found trigger file {File} in {Owner}/{Repo}",
        TriggerFileName,
        account,
        repo);

      return Deserialiser.Deserialize<TriggersList>(file.Content);
    }
    catch (NotFoundException)
    {
      Logger.LogWarning("No trigger file found in {Owner}/{Repo}", account, repo);
      return new TriggersList();
    }
  }

  private async Task TriggerDestinations(Trigger trigger, WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
  {
    Logger.LogInformation("Looking at trigger {Source}", trigger.Source);
    var filtered = trigger.Targets.Where(x => workflowRunEvent.Workflow.Path.EndsWith(trigger.Source.Workflow));
    await ParallelTriggerDestinations(filtered, workflowRunEvent, installClient);
  }

  private async Task ParallelTriggerDestinations(IEnumerable<RepositoryWorkflow> targets, WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
  {
    await Parallel.ForEachAsync(targets, async (target, cancellation) =>
    {
      await TriggerTarget(target, workflowRunEvent, installClient);
    });
  }

  private async Task TriggerTarget(RepositoryWorkflow target, WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
  {
    if (await TriggerTargetAllowed(target, workflowRunEvent, installClient) == false)
    {
      Logger.LogInformation("Triggering target {Target} is not allowed for workflow run {WorkflowRunId} in {Owner}/{Repo}",
        target.Workflow,
        workflowRunEvent.WorkflowRun.Id,
        workflowRunEvent.Repository.Owner.Login,
        target.Repository);

      await Issues.CreateFailedIssue(target, workflowRunEvent, installClient);
      return;
    }
    else
    {
      await ExecuteTriggerTarget(target, workflowRunEvent, installClient);
    }
  }

  private async Task<bool> TriggerTargetAllowed(RepositoryWorkflow target, WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
  {
    //Get the triggering file for the target repository
    var targetTriggersFiles = await GetTriggersLists(installClient, workflowRunEvent.Repository.Owner.Login, target.Repository);

    var inboundThatAreFromTheSource = targetTriggersFiles.Inbound.Where(inbound => inbound.Source.Repository == workflowRunEvent.Repository.Name && workflowRunEvent.Workflow.Path.EndsWith(inbound.Source.Workflow));

    var allowed = inboundThatAreFromTheSource.SelectMany(t => t.Targets.Where(a => a.Workflow == target.Workflow));

    if (!allowed.Any())
    {
      Logger.LogInformation("No allowed inbound triggers for target {Owner}/{TargetRepo} {Target} from {Owner}/{SourceRepo} {workflow}", workflowRunEvent.Repository.Owner.Login, target.Repository, target.Workflow, workflowRunEvent.Repository.Owner.Login, workflowRunEvent.Repository.Name, workflowRunEvent.Workflow.Path);
      return false;
    }
    else
    {
      Logger.LogInformation("Allowed inbound triggers found for target {Owner}/{TargetRepo} {Target} from {Owner}/{SourceRepo} {workflow}", workflowRunEvent.Repository.Owner.Login, target.Repository, target.Workflow, workflowRunEvent.Repository.Owner.Login, workflowRunEvent.Repository.Name, workflowRunEvent.Workflow.Path);
      return true;
    }
  }

  private async Task ExecuteTriggerTarget(RepositoryWorkflow target, WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
  {
    var dispatchTask = Dispatches.CreateDispatch(target, workflowRunEvent, installClient);
    var issueTask = Issues.CreateIssue(target, workflowRunEvent, installClient);
    await Task.WhenAll(dispatchTask, issueTask);
    Logger.LogInformation("Created Dispatch on {Target}", target.Workflow);
    Logger.LogInformation("Created Issue {Issue}", issueTask.Result.Id);
  }
}

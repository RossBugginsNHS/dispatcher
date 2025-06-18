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
    var installClient = await cs.GetInstallationClient(workflowRunEvent.Installation.Id);
    var sourceTriggerData = await GetSourceTriggersList(workflowRunEvent, installClient);
    await Parallel.ForEachAsync(sourceTriggerData.Triggers, async (trigger, cancellation) =>
    {
      await TriggerDestinations(trigger, workflowRunEvent, installClient);
    });
  }


  private async Task<TriggersList> GetSourceTriggersList(WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
  {
    var file = (await installClient.Repository.Content.GetAllContents(
            workflowRunEvent.Repository.Owner.Login,
            workflowRunEvent.Repository.Name,
            TriggerFileName)).First();
    return Deserialiser.Deserialize<TriggersList>(file.Content);
  }


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
    var dispatchTask = Dispatches.CreateDispatch(target, workflowRunEvent, installClient);
    var issueTask = Issues.CreateIssue(target, workflowRunEvent, installClient);
    await Task.WhenAll(dispatchTask, issueTask);
    Logger.LogInformation("Created Issue {Issue}", issueTask.Result.Id);
  }
}

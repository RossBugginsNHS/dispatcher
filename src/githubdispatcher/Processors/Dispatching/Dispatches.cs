using Octokit;
using Octokit.Webhooks.Events;

public class Dispatches
{
  public async Task CreateDispatch(RepositoryWorkflow target, WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
  {
    ArgumentNullException.ThrowIfNull(target, nameof(target));
    ArgumentNullException.ThrowIfNull(workflowRunEvent, nameof(workflowRunEvent));
    ArgumentNullException.ThrowIfNull(installClient, nameof(installClient));
    ArgumentNullException.ThrowIfNull(workflowRunEvent.Repository, nameof(workflowRunEvent.Repository));

    await installClient.Actions.Workflows.CreateDispatch(
      workflowRunEvent.Repository.Owner.Login,
      target.Repository,
      target.Workflow, new CreateWorkflowDispatch("main"));
  }
}

using Octokit;
using Octokit.Webhooks.Events;

public class Dispatches
{
  public async Task CreateDispatch(Target target, WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
  {
    await installClient.Actions.Workflows.CreateDispatch(
      workflowRunEvent.Repository.Owner.Login,
      target.Repository,
      target.Workflow, new CreateWorkflowDispatch("main"));
  }
}

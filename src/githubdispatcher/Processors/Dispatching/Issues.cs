using Octokit;
using Octokit.Webhooks.Events;

public class Issues
{
  public async Task<Issue> CreateIssue(RepositoryWorkflow target, WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
  {
    var title = $"Triggered a dispatch of {target.Repository} {target.Workflow}";
    return await installClient.Issue.Create(
      workflowRunEvent.Repository.Owner.Login,
      workflowRunEvent.Repository.Name,
      new NewIssue(title) { Body = "Triggering" });
  }

  public async Task<Issue> CreateFailedIssue(RepositoryWorkflow target, WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
  {
    var title = $"FAILED TO TRIGGER a dispatch of {target.Repository} {target.Workflow}";

    return await installClient.Issue.Create(
      workflowRunEvent.Repository.Owner.Login,
      workflowRunEvent.Repository.Name,
      new NewIssue(title) { Body = "Failed to trigger the target workflow as the target workflow does not give us permission to do so." });
  }
}

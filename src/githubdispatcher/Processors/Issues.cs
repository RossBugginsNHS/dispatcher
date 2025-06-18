using Octokit;
using Octokit.Webhooks.Events;

public class Issues
{
  public async Task<Issue> CreateIssue(Target target, WorkflowRunEvent workflowRunEvent, GitHubClient installClient)
  {
    var title = $"Triggered a dispatch of {target.Repository} {target.Workflow}";
    return await installClient.Issue.Create(
      workflowRunEvent.Repository.Owner.Login,
      workflowRunEvent.Repository.Name,
      new NewIssue(title) { Body = "Triggering" });
  }
}

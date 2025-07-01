using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.WorkflowRun;

public partial class GithubWorkflowEventProcessor : Octokit.Webhooks.WebhookEventProcessor
{
  private void ThrowIfNull(ILogger<GithubWorkflowEventProcessor> logger, ClientSetup cs, WorkFlowRunCompletedHandler runner)
  {
    ArgumentNullException.ThrowIfNull(logger, nameof(logger));
    ArgumentNullException.ThrowIfNull(cs, nameof(cs));
    ArgumentNullException.ThrowIfNull(runner, nameof(runner));
  }

  private void ThrowIfNull(WebhookHeaders headers, WorkflowRunEvent workflowRunEvent, WorkflowRunAction action)
  {
    ArgumentNullException.ThrowIfNull(headers, nameof(headers));
    ArgumentNullException.ThrowIfNull(workflowRunEvent, nameof(workflowRunEvent));
    ArgumentNullException.ThrowIfNull(action, nameof(action));
  }

  private void ThrowIfNull(WorkflowRunEvent workflowRunEvent, WorkflowRunAction action)
  {
    ArgumentNullException.ThrowIfNull(workflowRunEvent, nameof(workflowRunEvent));
    ArgumentNullException.ThrowIfNull(action, nameof(action));
  }

  private void ThrowIfNull(WorkflowRunEvent workflowRunEvent)
  {
    ArgumentNullException.ThrowIfNull(workflowRunEvent, nameof(workflowRunEvent));
  }

  private void ThrowIfNull(WorkflowRunAction action)
  {
    ArgumentNullException.ThrowIfNull(action, nameof(action));
  }
}


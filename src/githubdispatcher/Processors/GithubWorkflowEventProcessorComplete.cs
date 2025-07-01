using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.WorkflowRun;

public partial class GithubWorkflowEventProcessor : Octokit.Webhooks.WebhookEventProcessor
{
  private const bool IS_COMPLETED_WORKFLOW_ACTION = true;
  private const bool IS_NOT_COMPLETED_WORKFLOW_ACTION = false;

  private async Task HandleIfComplete(WorkflowRunEvent workflowRunEvent, WorkflowRunAction action)
  {
    ThrowIfNull(workflowRunEvent, action);
    await (IsCompletedWorkflowAction(action) switch
    {
      IS_COMPLETED_WORKFLOW_ACTION => HandleCompletedWorkflowAction(workflowRunEvent),
      IS_NOT_COMPLETED_WORKFLOW_ACTION => HandleNotCompletedWorkflowAction(workflowRunEvent),
    });
  }

  private bool IsCompletedWorkflowAction(WorkflowRunAction action)
  {
    ThrowIfNull(action);
    return (string)action == WorkflowRunActionValue.Completed;
  }

  private async Task HandleCompletedWorkflowAction(WorkflowRunEvent workflowRunEvent)
  {
    ThrowIfNull(workflowRunEvent);
    LogProcessingComplete(workflowRunEvent.WorkflowRun.Id);
    await _runner.HandleWorkFlowRunCompleted(workflowRunEvent);
    LogProcessedComplete(workflowRunEvent.WorkflowRun.Id);
  }

  private Task HandleNotCompletedWorkflowAction(WorkflowRunEvent workflowRunEvent)
  {
    LogIgnoringEvent();
    return Task.CompletedTask;
  }
}

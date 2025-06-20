using Octokit.Webhooks;
using Octokit.Webhooks.Events;

public partial class WorkFlowRunCompletedHandler(ILogger<WorkFlowRunCompletedHandler> Logger, Triggering Triggering)
{

  public async Task HandleWorkFlowRunCompleted(WorkflowRunEvent workflowRunEvent)
  {
    LogGotWebHookCall(workflowRunEvent);
    await Triggering.TriggerAll(workflowRunEvent);
  }



    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Got a webhook call of {Action} from {Owner}/{Repo}: {Workflow}")]
    protected static partial void LogGotWebHookCall(
      ILogger<WorkFlowRunCompletedHandler> logger,
      string action,
      string owner,
      string repo,
      string workflow);

  private void LogGotWebHookCall(WorkflowRunEvent workflowRunEvent)
  {
    ArgumentNullException.ThrowIfNull(workflowRunEvent, nameof(workflowRunEvent));
    ArgumentNullException.ThrowIfNull(workflowRunEvent.Repository, nameof(workflowRunEvent.Repository));
    ArgumentNullException.ThrowIfNull(workflowRunEvent.Workflow, nameof(workflowRunEvent.Workflow));
    ArgumentNullException.ThrowIfNull(workflowRunEvent.Installation, nameof(workflowRunEvent.Installation));
    ArgumentNullException.ThrowIfNull(workflowRunEvent.Action, nameof(workflowRunEvent.Action));

    LogGotWebHookCall(
      Logger,
      workflowRunEvent.Action,
      workflowRunEvent.Repository.Owner.Login,
      workflowRunEvent.Repository.Name,
      workflowRunEvent.Workflow.Name);
  }
  }

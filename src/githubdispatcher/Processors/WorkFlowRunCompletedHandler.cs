using Octokit.Webhooks;
using Octokit.Webhooks.Events;

public partial class WorkFlowRunCompletedHandler(ILogger<WorkFlowRunCompletedHandler> Logger, ClientSetup cs, Triggering Triggering)
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
    private void LogGotWebHookCall(WorkflowRunEvent workflowRunEvent) =>
      LogGotWebHookCall(
        Logger,
        workflowRunEvent.Action,
        workflowRunEvent.Repository.Owner.Login,
        workflowRunEvent.Repository.Name,
        workflowRunEvent.Workflow.Name);
  }

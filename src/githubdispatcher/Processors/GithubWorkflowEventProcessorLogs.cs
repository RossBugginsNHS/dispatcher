public partial class GithubWorkflowEventProcessor : Octokit.Webhooks.WebhookEventProcessor
{
  [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "GithubWorkflowEventProcessor initialized with ClientSetup and WorkFlowRunCompletedHandler.")]
  protected partial void LogInitialized();

  [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Processing WorkflowRunEvent: {Action} for WorkflowRunId: {WorkflowRunId}")]
  protected partial void LogProcessing(string action, long workflowRunId);

  [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Processing WorkflowRunEvent completed for WorkflowRunId: {WorkflowRunId}")]
  protected partial void LogProcessingComplete(long workflowRunId);

  [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "Processed WorkflowRunEvent completed for WorkflowRunId: {WorkflowRunId}")]
  protected partial void LogProcessedComplete(long workflowRunId);

  [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "GithubWorkflowEventProcessor initializing with ClientSetup and WorkFlowRunCompletedHandler.")]
  protected partial void LogInitializing();

  [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "Ignoring Event")]
  protected partial void LogIgnoringEvent();
}


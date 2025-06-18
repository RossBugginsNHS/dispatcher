using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.WorkflowRun;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Octokit.Webhooks.Events.Installation;
using Octokit.Webhooks.Events.InstallationRepositories;
using Octokit.Webhooks.Events.InstallationTarget;


public class GithubWorkflowEventProcessor : Octokit.Webhooks.WebhookEventProcessor
{

  private readonly ILogger<GithubWorkflowEventProcessor> _logger;
  private readonly ClientSetup _cs;
  private readonly WorkFlowRunCompletedHandler _runner;

  public GithubWorkflowEventProcessor(ILogger<GithubWorkflowEventProcessor> logger, ClientSetup cs, WorkFlowRunCompletedHandler runner) : base()
  {
    _logger = logger;
    _cs = cs;
    _runner = runner;
  }

  protected override async Task ProcessWorkflowRunWebhookAsync(
    WebhookHeaders headers,
    WorkflowRunEvent workflowRunEvent,
    WorkflowRunAction action)
  {
    if ((string)action == WorkflowRunActionValue.Completed)
    {
      await _runner.HandleWorkFlowRunCompleted(workflowRunEvent);
    }
    ;
  }
}


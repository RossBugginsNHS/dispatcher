using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.WorkflowRun;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Octokit.Webhooks.Events.Installation;
using Octokit.Webhooks.Events.InstallationRepositories;
using Octokit.Webhooks.Events.InstallationTarget;


public class MyDispaterWebhookEventProcessor : Octokit.Webhooks.WebhookEventProcessor
{

  ILogger<Runner> Logger;
  ClientSetup cs;
  public MyDispaterWebhookEventProcessor(ILogger<Runner> logger, ClientSetup cs) : base()
  {
    Logger = logger;
    this.cs = cs;
  }



  protected override async Task ProcessWorkflowRunWebhookAsync(
    WebhookHeaders headers,
    WorkflowRunEvent workflowRunEvent,
    WorkflowRunAction action)
  {
    if ((string)action == WorkflowRunActionValue.Completed)
    {
      var runner = new Runner(Logger, cs);
      await runner.HandleWorkFlowRunCompleted(workflowRunEvent);
    }
    ;
  }
}


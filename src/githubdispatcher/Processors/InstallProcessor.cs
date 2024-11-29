
using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.Installation;
using Octokit.Webhooks.Events.InstallationRepositories;
using Octokit.Webhooks.Events.InstallationTarget;
public partial  class WebhookEventProcessor(ILogger<WebhookEventProcessor> Logger, ClientSetup cs) : Octokit.Webhooks.WebhookEventProcessor
{
    protected override Task ProcessInstallationWebhookAsync(
        WebhookHeaders headers,
        InstallationEvent installationEvent,
        InstallationAction action)
    {
            Logger.LogInformation("ProcessInstallationWebhookAsync {Action} ",
            installationEvent.Action);

        return base.ProcessInstallationWebhookAsync(headers, installationEvent, action);
    }

    protected override async Task ProcessInstallationRepositoriesWebhookAsync(
        WebhookHeaders headers,
        InstallationRepositoriesEvent installationRepositoriesEvent,
        InstallationRepositoriesAction action)
    {
          Logger.LogInformation("ProcessInstallationRepositoriesWebhookAsync {Action}",
            installationRepositoriesEvent.Action);

        await base.ProcessInstallationRepositoriesWebhookAsync(headers, installationRepositoriesEvent, action);
    }

    protected override Task ProcessInstallationTargetWebhookAsync(
        WebhookHeaders headers,
        InstallationTargetEvent installationTargetEvent,
        InstallationTargetAction action)
    {
           Logger.LogInformation("ProcessInstallationTargetWebhookAsync {Action}",
            installationTargetEvent.Action);
        return base.ProcessInstallationTargetWebhookAsync(headers, installationTargetEvent, action);
    }

}

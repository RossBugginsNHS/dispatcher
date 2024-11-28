
// Add services to the container.

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.





using Microsoft.Extensions.Options;
using Octokit;
using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.Installation;
using Octokit.Webhooks.Events.InstallationRepositories;
using Octokit.Webhooks.Events.InstallationTarget;
using Org.BouncyCastle.Asn1.Mozilla;
public partial  class MyWebhookEventProcessor(ILogger<MyWebhookEventProcessor> Logger, ClientSetup cs) : WebhookEventProcessor
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

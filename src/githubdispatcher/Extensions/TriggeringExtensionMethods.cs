using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Octokit.Webhooks;
using Octokit.Webhooks.AspNetCore;

public static class TriggeringExtensionMethods
{
  public static IServiceCollection AddGitHubDispatcher(this IServiceCollection services) => services.AddGitHubDispatcher(x=>{});

  public static IServiceCollection AddGitHubDispatcher(this IServiceCollection services, Action<GitHubDispatcherOptions> options)
  {
    _ = services

      .AddSingleton<WebhookEventProcessor, MyDispaterWebhookEventProcessor>()
      .AddSingleton<ClientSetup>();


    _ = services.AddOptions<GitHubDispatcherOptions>().BindConfiguration(GitHubDispatcherOptions.Name).Configure(options);
    return services;
  }

  public static WebApplication UseGitHubDispatcher(this WebApplication app)
  {
    var settings = app.Services.GetRequiredService<IOptions<GitHubDispatcherOptions>>();

    app.MapGitHubWebhooks(settings.Value.UrlPath, settings.Value.Secret);

    app.MapGet("/", async (
      [FromServices] IHostEnvironment env,
      [FromServices] ClientSetup cs,
      [FromServices] IOptions<GitHubDispatcherOptions> Settings) =>
    {
      var appClient = cs.GetAppClient();
      var app = await appClient.GitHubApps.GetCurrent();
      var installs = await appClient.GitHubApps.GetAllInstallationsForCurrent();

      var o = new List<object>();
      await Parallel.ForEachAsync(installs, async (install, cancel) =>
  {
    var repos1 = install.RepositoriesUrl.ToList();
    var installClient = await cs.GetInstallationClient(appClient, install.Id);
    cancel.ThrowIfCancellationRequested();
    var repos = await installClient.GitHubApps.Installation.GetAllRepositoriesForCurrent();
    cancel.ThrowIfCancellationRequested();
    foreach (var repo in repos.Repositories)
    {
      o.Add(new { Owner = repo.Owner.Login, Name = repo.Name, Install = install.Id, App = install.AppId });
    }
    cancel.ThrowIfCancellationRequested();
  });

      return env.IsProduction() ?
   Results.Forbid() :
   TypedResults.Ok(new { summary = new { App = app.Id, AppName = app.Name }, Details = o });
    });
    return app;
  }
}

using System.Collections.Concurrent;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Octokit;
using Octokit.Webhooks;
using Octokit.Webhooks.AspNetCore;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var builder = WebApplication.CreateBuilder(args);
builder.Logging
  .ClearProviders()
  .AddConsole()
  .AddFilter(null, LogLevel.Trace)
  .AddFilter("*", LogLevel.Trace)
  .SetMinimumLevel(LogLevel.Trace);
builder.Services
  .AddSingleton<WebhookEventProcessor, MyWebhookEventProcessor>()
  .AddSingleton<ClientSetup>()
  .AddOptions<GitHubDispatcherOptions>().BindConfiguration(GitHubDispatcherOptions.Name);
builder.Services.AddControllers();
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);
var app = builder.Build();
app
  .UseHttpsRedirection()
  .UseAuthorization();
app.MapControllers();

var settings = app.Services.GetRequiredService<IOptions<GitHubDispatcherOptions>>();
app.MapGitHubWebhooks(settings.Value.UrlPath, settings.Value.Secret);

app.MapGet("/", async (
  [FromServices] IHostEnvironment   env,
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

await app.RunAsync();

using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Octokit;

public class InfoController(
 IHostEnvironment env,
  ClientSetup cs,
  ILogger<InfoController> logger,
  IOptions<GitHubDispatcherOptions> settings
)
{
  public async Task<IResult> GetInfo()

  {


    logger.LogInformation("Getting GitHub App information");
    var appClient = cs.GetAppClient();
    var app = await appClient.GitHubApps.GetCurrent();
    var installs = await appClient.GitHubApps.GetAllInstallationsForCurrent();

    var o = new ConcurrentBag<object>();
    await Parallel.ForEachAsync(installs, async (install, cancel) => await InstallInfo(appClient, install, o, cancel));

    logger.LogInformation("Got GitHub App information");

    return GetResult(app, o);
  }

  private IResult GetResult(GitHubApp app, ConcurrentBag<object> o)
  {
    return //env.IsProduction() ?
    //Results.Forbid() :
    TypedResults.Ok(new { summary = new { App = app.Id, AppName = app.Name }, Details = o });

  }

  private async Task InstallInfo(GitHubClient appClient, Installation install, ConcurrentBag<object> o, CancellationToken cancel)
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
  }
}


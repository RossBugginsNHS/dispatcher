using Microsoft.Extensions.Options;

public class InfoController(
 IHostEnvironment env,
  ClientSetup cs,
  ILogger<InfoController> logger,
  IOptions<GitHubDispatcherOptions> settings
)
{
  public async Task<IResult> GetInfo()

  {
    {
      logger.LogInformation("Getting GitHub App information");
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

  logger.LogInformation("Got GitHub App information");
      return env.IsProduction() ?
   Results.Forbid() :
   TypedResults.Ok(new { summary = new { App = app.Id, AppName = app.Name }, Details = o });
    }
  }
}


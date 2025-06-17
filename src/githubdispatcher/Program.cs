using System.Collections.Concurrent;
using System.Text;
using Octokit;
using Octokit.Webhooks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

await RunWebApp.Run(args);

public static class RunWebApp
{
  public static async Task Run(string[] args)
  {

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging
      .ClearProviders()
      .AddConsole()
      .AddFilter(null, LogLevel.Trace)
      .AddFilter("*", LogLevel.Trace)
      .SetMinimumLevel(LogLevel.Trace);

    builder.Services.AddGitHubDispatcher();

    builder.Services.AddControllers();
    var app = builder.Build();

    app
      .UseHttpsRedirection()
      .UseAuthorization();
    app.MapControllers();

    app.UseGitHubDispatcher();

    await app.RunAsync();
  }
}

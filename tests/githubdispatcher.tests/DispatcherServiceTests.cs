using githubdispatcher;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace githubdispatcher.tests;

public class UnitTest1
{
  [Fact]
  public async Task Test1()
  {
    var app = Prepare();
    Guid id = Guid.Empty;
    {

      var service = app.Services.GetRequiredService<TriggeringProcessService>();
      var agg = await service.AddSource(new CompletedWorkflowDetails("MyOwner", "MyRepo", "MyWorkflow"));
      id = agg.Id;
    }

    {
      var repo = app.Services.GetRequiredService<TriggeringProcessRepositoryFactory>().Create();
      var agg = repo.Get(id);
    }
  }

  private IHost Prepare()
  {
  var builder = new HostBuilder();
      builder.ConfigureServices(services =>
        services
        .AddLogging(logging => logging.AddConsole())
          .AddGitHubDispatcher());

      var app = builder.Build();
      return app;
  }
}

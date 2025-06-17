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
    Guid id = Guid.Empty;
    {
      var builder = new HostBuilder();
      builder.ConfigureServices(services =>
        services
        .AddLogging(logging => logging.AddConsole())
          .AddTransient<TriggeringProcessAggreagateRoot>()
          .AddTransient<TriggeringProcessRepository>()
          .AddTransient<TriggeringProcessService>()
          .AddTransient<TriggeringProcessRepositoryFactory>()
          .AddTransient<ISourceTriggerRepository, SourceTriggerRepository>()
          .AddTransient<ITriggerer, Triggerer>()
          .AddMediatR(c => c.RegisterServicesFromAssemblyContaining<TriggeringProcessService>()));
      var app = builder.Build();
      var service = app.Services.GetRequiredService<TriggeringProcessService>();
      var agg = await service.AddSource(new CompletedWorkflowDetails("MyOwner", "MyRepo", "MyWorkflow"));
      id = agg.Id;
    }

    {
      var builder = new HostBuilder();
      builder.ConfigureServices(services =>
        services
          .AddTransient<TriggeringProcessAggreagateRoot>()
            .AddTransient<TriggeringProcessRepositoryFactory>()
          .AddTransient<TriggeringProcessRepository>()
          .AddMediatR(c => c.RegisterServicesFromAssemblyContaining<TriggeringProcessService>()));
      var app = builder.Build();
      var repo = app.Services.GetRequiredService<TriggeringProcessRepositoryFactory>().Create();
      var agg = repo.Get(id);

    }
  }
}

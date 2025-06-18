public static class TestingStuff
{
  public static async Task Run(string[]? args)
  {


    var builder = Host.CreateApplicationBuilder(args);
    builder.Logging
      .ClearProviders()
      .AddConsole()
      .AddFilter(null, LogLevel.Trace)
      .AddFilter("*", LogLevel.Trace)
      .SetMinimumLevel(LogLevel.Trace);

    builder.Services.AddGitHubDispatcher();
    var app = builder.Build();

    var disp = app.Services.GetService<DispatcherUnitOfWorkFactory>();
    disp.Create();

    await app.RunAsync();


  }
}


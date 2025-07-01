public static class RunWebApp
{
  public static async Task Run(string[] args, Action<WebApplicationBuilder> configure = null )
  {
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging
      .AddConsole()
      .AddFilter(null, LogLevel.Trace)
      .AddFilter("*", LogLevel.Trace)
      .SetMinimumLevel(LogLevel.Trace);

    builder.Services.AddGitHubDispatcher(o =>
    {
     // o.UrlPath = "";
    });

    builder.Services.AddControllers();
    builder.Services.AddAuthentication();

    if(configure != null)
    {
      configure(builder);
    }

    var app = builder.Build();
    app.Services.GetService<ILogger<Program>>().LogInformation("Starting GitHub Dispatcher Web App");
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.MapControllers();
    app.UseGitHubDispatcher();
    app.UseAuthentication();
    app.UseAuthorization();
    await app.RunAsync();
  }
}

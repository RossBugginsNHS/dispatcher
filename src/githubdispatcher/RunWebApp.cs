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

    builder.Services.AddGitHubDispatcher(o =>
    {
     // o.UrlPath = "";
    });

    builder.Services.AddControllers();
    builder.Services.AddAuthentication();

    var app = builder.Build();

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.MapControllers();
    app.UseGitHubDispatcher();
    app.UseAuthentication();
    app.UseAuthorization();
    await app.RunAsync();
  }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Octokit.Webhooks;
using Octokit.Webhooks.AspNetCore;

public static partial class TriggeringExtensionMethods
{
  public static IServiceCollection AddGitHubDispatcher(this IServiceCollection services) => services.AddGitHubDispatcher(x => { });

  public static IServiceCollection AddGitHubDispatcher(this IServiceCollection services, Action<GitHubDispatcherOptions> options)
  {
    _ = services
      .AddNHSDisptcher()
      .AddNHSGitHubDisptcher()
      .AddNHSDisptcherOptions(options);

    return services;
  }

  public static void ValidateNHSDisptcherOptions(GitHubDispatcherOptions options)
  {
    _ = options.UrlPath ?? throw new ArgumentNullException(nameof(options.UrlPath), "UrlPath must be set");
    _ = options.Secret ?? throw new ArgumentNullException(nameof(options.Secret), "Secret must be set");
    _ = options.AppId ?? throw new ArgumentNullException(nameof(options.AppId), "AppId must be set");
    _ = options.AppHeader ?? throw new ArgumentNullException(nameof(options.AppHeader), "AppHeader must be set");
    _ = options.Pem ?? throw new ArgumentNullException(nameof(options.Pem), "Pem must be set");
    _ = options.PemContent ?? throw new ArgumentNullException(nameof(options.PemContent), "PemContent must be set");
    _ = options.TokenLifetime <= 0 ? throw new ArgumentNullException(nameof(options.PemContent), "PemContent must be more than 0") : 0;
  }

  public static void DefaultNHSDisptcherOptions(GitHubDispatcherOptions options)
  {

    options.SetPemContent(options.PemContent ?? File.ReadAllText(options.Pem));
  }

  public static IServiceCollection AddNHSDisptcherOptions(this IServiceCollection services, Action<GitHubDispatcherOptions> options)
  {
    var o = services
    .AddOptions<GitHubDispatcherOptions>()
    .BindConfiguration(GitHubDispatcherOptions.Name)
    .Configure(DefaultNHSDisptcherOptions)
    .Configure(options)
  .Configure(ValidateNHSDisptcherOptions);


    //  services.PostConfigure(ValidateNHSDisptcherOptions);
    return services;
  }
  public static IServiceCollection AddNHSDisptcher(this IServiceCollection services)
  {
    return services
      .AddTriggerer()
      .AddInfoController();
  }

  private static IServiceCollection AddInfoController(this IServiceCollection services)
  {
    return services
      .AddTransient<InfoController>();
  }

  private static IServiceCollection AddTriggerer(this IServiceCollection services)
  {
    return services
      .AddTransient<Issues>()
      .AddTransient<Dispatches>()
      .AddTransient<Triggering>()
      .AddTransient<WorkFlowRunCompletedHandler>();
  }


  public static IServiceCollection AddNHSGitHubDisptcher(this IServiceCollection services)
  {
    return services
      .AddSingleton<WebhookEventProcessor, GithubWorkflowEventProcessor>()
      .AddSingleton<ClientSetup>();
  }

  public static WebApplication UseGitHubDispatcher(this WebApplication app)
  {
    return app.MapGitHub()
      .MapGitHubDispatcherInfoPage();
  }

  public static WebApplication MapGitHub(this WebApplication app)
  {
    var settings = app.Services.GetRequiredService<IOptions<GitHubDispatcherOptions>>();
    app.MapGitHubWebhooks(settings.Value.UrlPath, settings.Value.Secret);
    return app;
  }

  public static WebApplication MapGitHubDispatcherInfoPage(this WebApplication app)
  {

    app.MapGet("/", async (
      [FromServices] InfoController infoController) => await infoController.GetInfo()
    );
    return app;
  }
}

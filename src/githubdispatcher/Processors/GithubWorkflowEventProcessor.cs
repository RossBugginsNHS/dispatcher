using Octokit;
using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.ContentReference;
using Octokit.Webhooks.Events.PullRequestReviewComment;
using Octokit.Webhooks.Events.WorkflowRun;
using Org.BouncyCastle.Crypto.Engines;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public partial class GithubWorkflowEventProcessor : Octokit.Webhooks.WebhookEventProcessor
{

  private readonly ILogger<GithubWorkflowEventProcessor> _logger;
  private readonly ClientSetup _cs;
  private readonly WorkFlowRunCompletedHandler _runner;
  private readonly IServiceProvider _services;

  public GithubWorkflowEventProcessor(IServiceProvider services, ILogger<GithubWorkflowEventProcessor> logger, ClientSetup cs, WorkFlowRunCompletedHandler runner) : base()
  {
    ThrowIfNull(logger, cs, runner);
    _logger = logger;
    LogInitializing();
    _services = services;
    _cs = cs;
    _runner = runner;
    LogInitialized();
  }

  protected override async Task ProcessWorkflowRunWebhookAsync(WebhookHeaders headers, WorkflowRunEvent workflowRunEvent, WorkflowRunAction action)
  {
    await CreateARepo(headers, workflowRunEvent, action);
    ThrowIfNull(headers, workflowRunEvent, action);
    LogProcessing(action, workflowRunEvent.WorkflowRun.Id);
    await HandleIfComplete(workflowRunEvent, action);
  }


  protected override async Task ProcessPushWebhookAsync(WebhookHeaders headers, PushEvent pushEvent)
  {
    var account = pushEvent.Repository.Owner.Login;
    var repo = pushEvent.Repository.Name;
    var installClient = await _cs.GetInstallationClient(pushEvent.Installation.Id);
    var fileName = "vending.yml";

    if (pushEvent.Repository.Name == "vending")
    {
      string content = null;
      try
      {
        var file = (await installClient.Repository.Content.GetAllContents(
          account,
          repo,
          fileName)).FirstOrDefault();

        content = file?.Content;
      }
      catch (NotFoundException)
      {
        _logger.LogInformation("No vending.yml found in {Repo} for {Account}", repo, account);
      }

      if (content != null)
      {
        var deserialiser = new DeserializerBuilder()
          .WithNamingConvention(CamelCaseNamingConvention.Instance)
          .Build();

        var repos = deserialiser.Deserialize<RepoVending>(content);

       var appClient = _cs.GetAppClient();
      var app = await appClient.GitHubApps.GetCurrent();


        var owner = pushEvent.Repository.Owner.Login;
        var appId = app.Id.ToString();
        var installId = pushEvent.Installation.Id.ToString();
        var pem = "robu6-dispatcher.2024-11-14.private-key.pem";
        var rd = _services.GetService<RunningDocker>();


        await rd.InitTF(owner, repos);
        await rd.PlanTf(owner, appId, installId, pem);
        await rd.ApplyTf(owner);
      }
    }
  }

  public record RepoVending(
    List<RepoVendingRepo> Repositories,
    List<RepoTeam> RepoTeams,
    List<RepoTeamAssignment> TeamAssignments,
    List<RepoTeamMembership> TeamMemberships)
  {
    public RepoVending() : this(
      new List<RepoVendingRepo>(),
      new List<RepoTeam>(),
      new List<RepoTeamAssignment>(),
      new List<RepoTeamMembership>())
    {

    }
  }

  public record RepoVendingRepo(string Name, string Description)
  {
    public RepoVendingRepo() : this(string.Empty, string.Empty)
    {

    }
  }

  public record RepoTeam(string TeamName)
  {
    public RepoTeam() : this(string.Empty)
    {

    }
  }

  public record RepoTeamAssignment(string TeamName, string RepoName, string Role)
  {
    public RepoTeamAssignment() : this(string.Empty, string.Empty, string.Empty)
    {

    }
  }

  public record RepoTeamMembership(string TeamName, string UserName)
  {
    public RepoTeamMembership() : this(string.Empty, string.Empty)
    {

    }
  }


  private async Task CreateARepo(WebhookHeaders headers, WorkflowRunEvent workflowRunEvent, WorkflowRunAction action)
  {
    var random = Random.Shared.Next(0, int.MaxValue);
    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    var owner = workflowRunEvent.Repository.Owner.Name;
    var appClient = _cs.GetAppClient();
    var newRepo = new NewRepository($"some-new-repo-{timestamp}-{random}")
    {
      AutoInit = true,
      Description = "This is a new repository created by the GitHub Dispatcher.",
      Private = false,
      HasIssues = true,
      HasProjects = true,
      HasWiki = true
    };

    var newRepoStatus = await appClient.Repository.Create(owner, newRepo);
    _logger.LogInformation("Created new repository {RepoName} for owner {Owner} with id {Id}",
      newRepoStatus.Name, owner, newRepoStatus.Id);
  }
}

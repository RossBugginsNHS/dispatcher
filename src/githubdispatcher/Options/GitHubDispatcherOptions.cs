public record GitHubDispatcherOptions(
  string UrlPath,
  string Secret,
  string AppId,
  string AppHeader,
  string Pem,
  int TokenLifetime)
{
  public GitHubDispatcherOptions() : this(default, default, default, default, default, default)
  {

  }
  public static string Name = "GITHUB_DISPATCHER";
}

public record GitHubDispatcherOptions(
  string UrlPath,
  string Secret,
  string AppId,
  string AppHeader,
  string Pem,
  int TokenLifetime)
{

string _pemContent = default;
 public string PemContent => _pemContent;
  public GitHubDispatcherOptions() : this(default, default, default, default, default, default)
  {
  }

  public GitHubDispatcherOptions SetPemContent(string pemContent)
  {
    _pemContent = pemContent ?? throw new ArgumentNullException(nameof(pemContent), "PemContent cannot be null.");
    return this;
  }

  public static string Name = "GITHUB_DISPATCHER";
}

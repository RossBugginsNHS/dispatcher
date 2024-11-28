// Add services to the container.

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.



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

// public class GitHubDispatcherOptions
// {
//     public static string Name = "GITHUB_DISPATCHER";
//     public string UrlPath {get;init;}
//     public string Secret {get;init;}
//     public string AppId{get;init;}
//     public string AppHeader {get;init;}
//     public string Pem {get;init;}

//     public int TokenLifetime {get;init;}

// }

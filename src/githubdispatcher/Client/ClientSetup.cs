
// Add services to the container.

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.





using System.Security.Cryptography;
using Octokit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

public class ClientSetup
{
  private readonly ILogger<ClientSetup> _logger;
  private readonly IOptions<GitHubDispatcherOptions> _dispatcherOptions;
  private readonly int _tokenLifetime;
  private readonly string _rsaPrivateKey;
  private readonly string _appId;
  private readonly string _appHeader;

  public ClientSetup(ILogger<ClientSetup> logger, IOptions<GitHubDispatcherOptions> dispatcherOptions)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
    _dispatcherOptions = dispatcherOptions ?? throw new ArgumentNullException(nameof(dispatcherOptions), "Dispatcher options cannot be null.");
    LogGitHubDispatcherOptions(dispatcherOptions?.Value);
    var options = dispatcherOptions?.Value ?? throw new ArgumentNullException(nameof(dispatcherOptions.Value), "Dispatcher options value cannot be null.");
    _tokenLifetime = options.TokenLifetime;
    _rsaPrivateKey = GetPemContent(dispatcherOptions);
    _appId = options.AppId;
    _appHeader = options.AppHeader;
  }

  public void LogGitHubDispatcherOptions(GitHubDispatcherOptions ? options)
  {
    if (options == null)
    {
      _logger.LogError("GitHubDispatcherOptions is null.");
      return;
    }
    _logger.LogInformation("GitHubDispatcherOptions:");
    _logger.LogInformation($"UrlPath: {options.UrlPath}");
    _logger.LogInformation($"Secret: {options.Secret}");
    _logger.LogInformation($"AppId: {options.AppId}");
    _logger.LogInformation($"AppHeader: {options.AppHeader}");
    _logger.LogInformation($"Pem: {options.Pem}");
    _logger.LogInformation($"TokenLifetime: {options.TokenLifetime}");
  }

  public static string GetPemContent(IOptions<GitHubDispatcherOptions> dispatcherOptions)
  {
    return dispatcherOptions.Value.PemContent ?? File.ReadAllText(dispatcherOptions.Value.Pem);
  }

  public GitHubClient GetAppClient()
  {
    var jwtToken = Token();
    var tokenAuth = new Credentials(jwtToken, AuthenticationType.Bearer);
    var appClient = new GitHubClient(new ProductHeaderValue(_appHeader))
    {
      Credentials = tokenAuth
    };
    return appClient;
  }

  public async Task<GitHubClient> GetInstallationClient(long installationId)
  {
    var appClient = this.GetAppClient();
    var install = await appClient.GitHubApps.GetInstallationForCurrent(installationId);
    var installClient = await this.GetInstallationClient(appClient, install.Id);
    return installClient;
  }

  public async Task<GitHubClient> GetInstallationClient(GitHubClient appClient, long installId)
  {
    var response = await appClient.GitHubApps.CreateInstallationToken(installId);
    var installationClient = new GitHubClient(new ProductHeaderValue(_appHeader))
    {
      Credentials = new Credentials(response.Token)
    };
    return installationClient;
  }

  public string Token()
  {
    using var rsa = RSA.Create();
    rsa.ImportFromPem(_rsaPrivateKey);
    var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
    {
      CryptoProviderFactory = new CryptoProviderFactory
      {
        CacheSignatureProviders = false
      }
    };

    var now = DateTime.UtcNow;
    var expiresAt = now + TimeSpan.FromSeconds(_tokenLifetime);
    var jwt = new JwtSecurityToken(
        notBefore: now,
        expires: now + TimeSpan.FromMinutes(10),
        signingCredentials: signingCredentials,
        claims: new[]
        {
        new Claim("iat", new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer),
        new Claim("iss", _appId.ToString(), ClaimValueTypes.Integer),
        }
    );
    var token = new JwtSecurityTokenHandler().WriteToken(jwt);
    return token;
  }
}

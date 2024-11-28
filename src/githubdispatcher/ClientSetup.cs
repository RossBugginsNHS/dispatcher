
// Add services to the container.

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.





using System.Security.Cryptography;
using Octokit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

public class ClientSetup(IOptions<GitHubDispatcherOptions> dispatcherOptions)
{
    private readonly int tokenLifetime = dispatcherOptions.Value.TokenLifetime;
    private readonly string rsaPrivateKey = File.ReadAllText(dispatcherOptions.Value.Pem);
    private readonly string appId = dispatcherOptions.Value.AppId;
    private readonly string appHeader =  dispatcherOptions.Value.AppHeader;

    public GitHubClient GetAppClient()
    {
        var jwtToken = Token();
        var tokenAuth = new Credentials(jwtToken, AuthenticationType.Bearer);
        var appClient = new GitHubClient(new ProductHeaderValue(appHeader))
        {
            Credentials = tokenAuth
        };
        return appClient;
    }

    public async Task<GitHubClient> GetInstallationClient(GitHubClient appClient, long installId)
    {
        var response = await appClient.GitHubApps.CreateInstallationToken(installId);
        var installationClient = new GitHubClient(new ProductHeaderValue(appHeader))
        {
            Credentials = new Credentials(response.Token)
        };
        return installationClient;
    }

    public string Token()
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(rsaPrivateKey);
        var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
        {
            CryptoProviderFactory = new CryptoProviderFactory
            {
                CacheSignatureProviders = false
            }
        };

        var now = DateTime.UtcNow;
        var expiresAt = now + TimeSpan.FromSeconds(tokenLifetime);
        var jwt = new JwtSecurityToken(
            notBefore: now,
            expires: now + TimeSpan.FromMinutes(10),
            signingCredentials: signingCredentials,
            claims: new[]
            {
        new Claim("iat", new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer),
        new Claim("iss", appId.ToString(), ClaimValueTypes.Integer),
            }
        );
        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return token;
    }
}

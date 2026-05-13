using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace CrmSolid.Auth;

/// <summary>
/// Bearer-token credentials for the public REST API v1 (<c>/v1/*</c>) and the MCP
/// endpoint (<c>/mcp</c>). Token format: <c>csk_{env}_{12-char-keyId}{32-char-secret}</c>.
/// </summary>
public sealed class BearerCredentials : ICrmSolidCredentials
{
    private readonly string _token;

    /// <param name="apiKey">The full bearer token, starting with <c>csk_</c>.</param>
    public BearerCredentials(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey));
        if (!apiKey.StartsWith("csk_", StringComparison.Ordinal))
            throw new ArgumentException(
                "API key must start with 'csk_'. Obtain a key from the CRM Solid dashboard.",
                nameof(apiKey));

        _token = apiKey;
    }

    /// <inheritdoc />
    public Task ApplyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        return Task.CompletedTask;
    }
}

using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Http;
using CrmSolid.Models;

namespace CrmSolid.Resources;

/// <summary>
/// Read-only connected-account inventory (Telegram + Twitter/X). Backed by
/// <c>/v1/accounts</c>. Requires <c>contacts:read</c>. The returned ids are the
/// <c>accountId</c> / <c>xAccountId</c> needed to send messages.
/// </summary>
public sealed class AccountsResource
{
    private readonly CrmSolidHttpClient _http;

    internal AccountsResource(CrmSolidHttpClient http) { _http = http; }

    /// <summary>Lists connected accounts, optionally filtered by type ("telegram" or "twitter").</summary>
    public Task<AccountList> ListAsync(string? type = null, CancellationToken cancellationToken = default)
    {
        var qs = new QueryStringBuilder();
        if (!string.IsNullOrEmpty(type)) qs.Add("type", type!);
        return _http.GetAsync<AccountList>("v1/accounts" + qs, cancellationToken);
    }
}

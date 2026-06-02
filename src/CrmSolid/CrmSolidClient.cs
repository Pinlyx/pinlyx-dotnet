using System;
using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.Options;
using CrmSolid.Auth;
using CrmSolid.Http;
using CrmSolid.Mcp;
using CrmSolid.Resources;

namespace CrmSolid;

/// <summary>
/// Primary entry point for the CRM Solid .NET SDK. Exposes resource groups
/// (<see cref="Me"/>, <see cref="Contacts"/>, <see cref="TelegramMessages"/>, <see cref="Mcp"/>)
/// over a shared <see cref="HttpClient"/>.
/// </summary>
/// <remarks>
/// <para>For most apps, construct with a bearer token:
/// <code>var client = new CrmSolidClient("csk_live_...");</code></para>
/// <para>For ASP.NET Core / DI, register via <c>services.AddCrmSolid(...)</c>
/// from <c>CrmSolid.Extensions</c>.</para>
/// </remarks>
public sealed class CrmSolidClient
{
    /// <summary>Workspace identity (<c>GET /v1/me</c>).</summary>
    public MeResource Me { get; }

    /// <summary>CRM contacts (<c>/v1/contacts</c>).</summary>
    public ContactsResource Contacts { get; }

    /// <summary>Outbound Telegram messaging (<c>/v1/telegram/messages</c>).</summary>
    public TelegramMessagesResource TelegramMessages { get; }

    /// <summary>Sales pipeline deals (<c>/v1/deals</c>).</summary>
    public DealsResource Deals { get; }

    /// <summary>CRM tasks / reminders (<c>/v1/tasks</c>).</summary>
    public TasksResource Tasks { get; }

    /// <summary>Read-only finance + revenue (<c>/v1/finance/*</c>).</summary>
    public FinanceResource Finance { get; }

    /// <summary>Email inbox threads (<c>/v1/email/threads</c>).</summary>
    public EmailResource Email { get; }

    /// <summary>MCP JSON-RPC endpoint (<c>POST /mcp</c>).</summary>
    public McpClient Mcp { get; }

    /// <summary>
    /// Construct with just a bearer API key. Builds an internal <see cref="HttpClient"/>
    /// with sensible defaults. Suitable for short scripts; for long-running apps
    /// prefer the DI registration to share a single <see cref="HttpClient"/>.
    /// </summary>
    public CrmSolidClient(string apiKey)
        : this(new CrmSolidOptions { ApiKey = apiKey }) { }

    /// <summary>Construct with explicit options. Builds an internal <see cref="HttpClient"/>.</summary>
    public CrmSolidClient(CrmSolidOptions options)
        : this(BuildHttpClient(options ?? throw new ArgumentNullException(nameof(options))), options) { }

    /// <summary>
    /// Construct from an already-configured <see cref="HttpClient"/>. Use this when
    /// you want to share the client (e.g., via <see cref="IHttpClientFactory"/>).
    /// </summary>
    public CrmSolidClient(HttpClient httpClient, CrmSolidOptions options)
    {
        if (httpClient is null) throw new ArgumentNullException(nameof(httpClient));
        if (options is null) throw new ArgumentNullException(nameof(options));

        if (httpClient.BaseAddress is null)
            httpClient.BaseAddress = options.BaseAddress;

        var inner = new CrmSolidHttpClient(httpClient);
        Me = new MeResource(inner);
        Contacts = new ContactsResource(inner);
        TelegramMessages = new TelegramMessagesResource(inner);
        Deals = new DealsResource(inner);
        Tasks = new TasksResource(inner);
        Finance = new FinanceResource(inner);
        Email = new EmailResource(inner);
        Mcp = new McpClient(inner);
    }

    /// <summary>DI-friendly overload that resolves options from <see cref="IOptions{TOptions}"/>.</summary>
    public CrmSolidClient(HttpClient httpClient, IOptions<CrmSolidOptions> options)
        : this(httpClient, (options ?? throw new ArgumentNullException(nameof(options))).Value) { }

    private static HttpClient BuildHttpClient(CrmSolidOptions options)
    {
        var credentials = ResolveCredentials(options);

        HttpMessageHandler chain = new HttpClientHandler();
        var auth = new CrmSolidAuthHandler(credentials) { InnerHandler = chain };
        var rate = new RateLimitHandler(options) { InnerHandler = auth };

        var http = new HttpClient(rate)
        {
            BaseAddress = options.BaseAddress,
            Timeout = options.Timeout,
        };
        http.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent ?? DefaultUserAgent);
        return http;
    }

    internal static ICrmSolidCredentials ResolveCredentials(CrmSolidOptions options)
    {
        if (!string.IsNullOrEmpty(options.ApiKey))
            return new BearerCredentials(options.ApiKey!);
        if (!string.IsNullOrEmpty(options.HmacKeyId) && !string.IsNullOrEmpty(options.HmacSecret))
            return new HmacCredentials(options.HmacKeyId!, options.HmacSecret!);

        throw new InvalidOperationException(
            "CrmSolidOptions: configure ApiKey (for /v1/*) or HmacKeyId + HmacSecret (for /public/*).");
    }

    internal static string DefaultUserAgent { get; } = BuildDefaultUserAgent();

    private static string BuildDefaultUserAgent()
    {
        var version = typeof(CrmSolidClient).GetTypeInfo().Assembly.GetName().Version?.ToString(3) ?? "0.0.0";
        return $"CrmSolid-dotnet/{version}";
    }
}

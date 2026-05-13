using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CrmSolid.Auth;
using CrmSolid.Http;

namespace CrmSolid.Extensions;

/// <summary>
/// DI registration helpers. Register the SDK once with
/// <c>services.AddCrmSolid(o =&gt; o.ApiKey = "csk_live_...")</c> and inject
/// <see cref="CrmSolidClient"/> wherever you need it.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="CrmSolidClient"/> and supporting services for DI.
    /// Uses <see cref="IHttpClientFactory"/> so the SDK plays nicely with
    /// pooled connections and handler lifetimes.
    /// </summary>
    public static IHttpClientBuilder AddCrmSolid(
        this IServiceCollection services,
        Action<CrmSolidOptions> configure)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (configure is null) throw new ArgumentNullException(nameof(configure));

        services.AddOptions<CrmSolidOptions>().Configure(configure);

        services.AddTransient<ICrmSolidCredentials>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<CrmSolidOptions>>().Value;
            return CrmSolidClient.ResolveCredentials(opts);
        });
        services.AddTransient<CrmSolidAuthHandler>();
        services.AddTransient<RateLimitHandler>();

        var builder = services.AddHttpClient<CrmSolidClient>((sp, http) =>
            {
                var opts = sp.GetRequiredService<IOptions<CrmSolidOptions>>().Value;
                http.BaseAddress = opts.BaseAddress;
                http.Timeout = opts.Timeout;
                http.DefaultRequestHeaders.UserAgent.ParseAdd(
                    opts.UserAgent ?? CrmSolidClient.DefaultUserAgent);
            })
            .AddHttpMessageHandler<RateLimitHandler>()
            .AddHttpMessageHandler<CrmSolidAuthHandler>();

        return builder;
    }
}

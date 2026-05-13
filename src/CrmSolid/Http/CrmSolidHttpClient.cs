using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Exceptions;
using CrmSolid.Models;

namespace CrmSolid.Http;

/// <summary>
/// Thin wrapper over <see cref="HttpClient"/> that handles JSON serialization,
/// error envelope parsing, and exception mapping for the CRM Solid API.
/// </summary>
internal sealed class CrmSolidHttpClient
{
    private readonly HttpClient _http;

    public CrmSolidHttpClient(HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public HttpClient Inner => _http;

    public async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, path);
        return await SendAndParseAsync<T>(req, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TResponse> PostJsonAsync<TRequest, TResponse>(
        string path,
        TRequest body,
        CancellationToken cancellationToken)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = CreateJsonContent(body)
        };
        return await SendAndParseAsync<TResponse>(req, cancellationToken).ConfigureAwait(false);
    }

    private static StringContent CreateJsonContent<T>(T body)
    {
        var json = JsonSerializer.Serialize(body, CrmSolidJson.Default);
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };
        return content;
    }

    private async Task<T> SendAndParseAsync<T>(
        HttpRequestMessage req,
        CancellationToken cancellationToken)
    {
        var response = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        try
        {
            if (!response.IsSuccessStatusCode)
                throw await CreateExceptionAsync(response, cancellationToken).ConfigureAwait(false);

            using var stream = await ReadAsStreamAsync(response, cancellationToken).ConfigureAwait(false);
            if (stream.CanSeek && stream.Length == 0)
            {
#pragma warning disable CS8603
                return default!;
#pragma warning restore CS8603
            }

            var result = await JsonSerializer
                .DeserializeAsync<T>(stream, CrmSolidJson.Default, cancellationToken)
                .ConfigureAwait(false);

            if (result is null)
                throw new CrmSolidException(
                    $"API returned a successful status but a null/empty body for response type {typeof(T).Name}.");

            return result;
        }
        finally
        {
            response.Dispose();
        }
    }

    private static Task<Stream> ReadAsStreamAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
#if NET5_0_OR_GREATER
        return response.Content.ReadAsStreamAsync(cancellationToken);
#else
        return response.Content.ReadAsStreamAsync();
#endif
    }

    private static async Task<CrmSolidException> CreateExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        ApiError? error = null;
        string? requestId = null;

        if (response.Headers.TryGetValues("X-Request-Id", out var rid))
            requestId = rid.FirstOrDefault();

        try
        {
            var contentType = response.Content?.Headers.ContentType?.MediaType;
            if (string.Equals(contentType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                using var stream = await ReadAsStreamAsync(response, cancellationToken).ConfigureAwait(false);
                if (stream.CanSeek ? stream.Length > 0 : true)
                {
                    error = await JsonSerializer
                        .DeserializeAsync<ApiError>(stream, CrmSolidJson.Default, cancellationToken)
                        .ConfigureAwait(false);
                }
            }
        }
        catch
        {
            // ignore parse failures — we still throw with whatever we have
        }

        var message = !string.IsNullOrEmpty(error?.Message)
            ? error!.Message
            : $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";

        return (int)response.StatusCode switch
        {
            400 => new CrmSolidValidationException(error, requestId, message),
            401 => new CrmSolidAuthException(error, requestId, message),
            403 => new CrmSolidForbiddenException(error, requestId, message),
            404 => new CrmSolidNotFoundException(error, requestId, message),
            429 => BuildRateLimit(response, error, requestId, message),
            _ => new CrmSolidApiException(response.StatusCode, error, requestId, message),
        };
    }

    private static CrmSolidRateLimitException BuildRateLimit(
        HttpResponseMessage response,
        ApiError? error,
        string? requestId,
        string message)
    {
        TimeSpan? retryAfter = null;
        if (response.Headers.RetryAfter?.Delta.HasValue == true)
            retryAfter = response.Headers.RetryAfter.Delta;
        else if (response.Headers.RetryAfter?.Date.HasValue == true)
            retryAfter = response.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow;

        return new CrmSolidRateLimitException(
            error,
            requestId,
            message,
            retryAfter,
            TryParseInt(response, "X-RateLimit-Limit"),
            TryParseInt(response, "X-RateLimit-Remaining"),
            TryParseLong(response, "X-RateLimit-Reset"));
    }

    private static int? TryParseInt(HttpResponseMessage response, string headerName)
    {
        if (!response.Headers.TryGetValues(headerName, out var vals)) return null;
        var v = vals.FirstOrDefault();
        return int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;
    }

    private static long? TryParseLong(HttpResponseMessage response, string headerName)
    {
        if (!response.Headers.TryGetValues(headerName, out var vals)) return null;
        var v = vals.FirstOrDefault();
        return long.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;
    }
}

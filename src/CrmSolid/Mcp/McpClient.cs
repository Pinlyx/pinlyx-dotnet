using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CrmSolid.Exceptions;
using CrmSolid.Http;

namespace CrmSolid.Mcp;

/// <summary>
/// Minimal client for the CRM Solid MCP endpoint (<c>POST /mcp</c>). Speaks
/// JSON-RPC 2.0 over the same bearer-authenticated channel as the REST API.
/// </summary>
/// <remarks>
/// This is a thin transport wrapper, not a full MCP protocol implementation.
/// For ergonomic tool listing / calling, see <see cref="ListToolsAsync"/> and
/// <see cref="CallToolAsync"/>; for anything else, use <see cref="SendAsync"/>.
/// </remarks>
public sealed class McpClient
{
    private readonly CrmSolidHttpClient _http;

    internal McpClient(CrmSolidHttpClient http)
    {
        _http = http;
    }

    /// <summary>Sends a raw JSON-RPC request to the MCP endpoint.</summary>
    public async Task<JsonElement> SendAsync(
        string method,
        object? @params = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(method))
            throw new ArgumentException("method required", nameof(method));

        var rpc = new McpJsonRpcRequest
        {
            JsonRpc = "2.0",
            Id = Guid.NewGuid().ToString("N"),
            Method = method,
            Params = @params,
        };

        var response = await _http
            .PostJsonAsync<McpJsonRpcRequest, McpJsonRpcResponse>("mcp", rpc, cancellationToken)
            .ConfigureAwait(false);

        if (response.Error != null)
        {
            throw new CrmSolidException(
                $"MCP error {response.Error.Code}: {response.Error.Message}");
        }

        return response.Result ?? default;
    }

    /// <summary>Returns the list of MCP tools exposed by the server (<c>tools/list</c>).</summary>
    public Task<JsonElement> ListToolsAsync(CancellationToken cancellationToken = default)
        => SendAsync("tools/list", null, cancellationToken);

    /// <summary>Invokes a named MCP tool with the given arguments (<c>tools/call</c>).</summary>
    public Task<JsonElement> CallToolAsync(
        string toolName,
        object? arguments = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toolName))
            throw new ArgumentException("toolName required", nameof(toolName));

        return SendAsync(
            "tools/call",
            new { name = toolName, arguments },
            cancellationToken);
    }
}

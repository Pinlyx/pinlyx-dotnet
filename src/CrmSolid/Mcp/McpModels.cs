using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrmSolid.Mcp;

/// <summary>JSON-RPC 2.0 request envelope used by the MCP endpoint.</summary>
internal sealed class McpJsonRpcRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public object? Params { get; set; }
}

/// <summary>JSON-RPC 2.0 response envelope returned by the MCP endpoint.</summary>
public sealed class McpJsonRpcResponse
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("result")]
    public JsonElement? Result { get; set; }

    [JsonPropertyName("error")]
    public McpJsonRpcError? Error { get; set; }
}

/// <summary>JSON-RPC 2.0 error envelope.</summary>
public sealed class McpJsonRpcError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public JsonElement? Data { get; set; }
}

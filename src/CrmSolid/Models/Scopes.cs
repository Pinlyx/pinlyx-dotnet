namespace CrmSolid.Models;

/// <summary>
/// String constants for the scopes that may be granted to an API key.
/// Use these instead of magic strings when inspecting <see cref="ApiKeyMetadata.Scopes"/>.
/// </summary>
public static class Scopes
{
    public const string ContactsRead = "contacts:read";
    public const string ContactsWrite = "contacts:write";
    public const string TelegramSend = "telegram:send";
    public const string TelegramRead = "telegram:read";
}

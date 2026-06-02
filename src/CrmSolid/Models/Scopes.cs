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
    public const string TwitterSend = "twitter:send";
    public const string SequencesRead = "sequences:read";
    public const string SequencesWrite = "sequences:write";
    public const string AnalyticsRead = "analytics:read";

    // Finance + Revenue (read-only), pipeline, tasks and the email inbox.
    public const string FinanceRead = "finance:read";
    public const string DealsRead = "deals:read";
    public const string DealsWrite = "deals:write";
    public const string TasksRead = "tasks:read";
    public const string TasksWrite = "tasks:write";
    public const string EmailRead = "email:read";
    public const string EmailWrite = "email:write";
}

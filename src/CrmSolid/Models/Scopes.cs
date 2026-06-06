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

    // Pipelines, self-service webhooks, AI agents and the job monitor.
    public const string PipelinesRead = "pipelines:read";
    public const string WebhooksRead = "webhooks:read";
    public const string WebhooksWrite = "webhooks:write";
    public const string AgentsRead = "agents:read";
    public const string AgentsRun = "agents:run";
    public const string JobsRead = "jobs:read";

    // Powerful Tier-3 scopes — NOT granted by default; attach them deliberately.
    // finance:write books ledger entries, email:send delivers mail, keys:manage
    // mints/revokes API keys (a key with this can create other keys).
    public const string FinanceWrite = "finance:write";
    public const string EmailSend = "email:send";
    public const string KeysManage = "keys:manage";
}

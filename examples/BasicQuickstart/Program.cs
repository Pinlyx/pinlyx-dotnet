using CrmSolid;

// Quickstart: authenticate, hit /v1/me, list the first page of contacts.

var apiKey = Environment.GetEnvironmentVariable("CRMSOLID_API_KEY")
    ?? throw new InvalidOperationException(
        "Set the CRMSOLID_API_KEY environment variable to a CRM Solid API key (csk_live_...).");

var client = new CrmSolidClient(apiKey);

var me = await client.Me.GetAsync();
Console.WriteLine($"Authenticated as user {me.Id} ({me.Email}).");
if (me.ApiKey is { } key)
    Console.WriteLine($"  keyId={key.KeyId} scopes=[{string.Join(", ", key.Scopes)}]");

var page = await client.Contacts.ListAsync(limit: 10);
Console.WriteLine($"\nFirst {page.Items.Count} contact(s) (hasMore={page.HasMore}):");
foreach (var c in page.Items)
{
    Console.WriteLine(
        $"  #{c.Id,-6} [{c.Platform,-8}] {c.Name ?? "(no name)",-25}  @{c.Username ?? "—"}  — {c.Stage}");
}

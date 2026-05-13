using CrmSolid;
using CrmSolid.Models;

// Stream every contact in the workspace, on demand, across cursor pages.
// Useful for mirroring CRM Solid contacts to another system, exporting to CSV, etc.

var apiKey = Environment.GetEnvironmentVariable("CRMSOLID_API_KEY")
    ?? throw new InvalidOperationException("Set CRMSOLID_API_KEY.");

var client = new CrmSolidClient(apiKey);

var sw = System.Diagnostics.Stopwatch.StartNew();
var total = 0;
var telegram = 0;
var twitter = 0;

await foreach (var contact in client.Contacts.StreamAllAsync(pageSize: 100))
{
    total++;
    if (contact.Platform == Platform.Telegram) telegram++;
    else if (contact.Platform == Platform.Twitter) twitter++;

    if (total % 250 == 0)
        Console.WriteLine($"  processed {total,6} contacts in {sw.Elapsed.TotalSeconds:F1}s …");
}

Console.WriteLine($"\nDone — {total} contact(s) in {sw.Elapsed.TotalSeconds:F1}s.");
Console.WriteLine($"  telegram: {telegram}");
Console.WriteLine($"  twitter : {twitter}");

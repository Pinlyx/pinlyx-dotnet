using CrmSolid;
using CrmSolid.Models;

// Send a Telegram message and poll until it leaves the queue.
//
// Required env:
//   CRMSOLID_API_KEY                  — Bearer key with scope telegram:send (+telegram:read for polling)
//   CRMSOLID_TELEGRAM_ACCOUNT_ID      — numeric id of a Telegram account you own
// Usage:
//   dotnet run -- <recipient-username>   (or pass a numeric Telegram user id)

if (args.Length < 1)
{
    Console.Error.WriteLine("usage: dotnet run -- <recipient-username-or-numeric-id>");
    return 1;
}

var apiKey = Environment.GetEnvironmentVariable("CRMSOLID_API_KEY")
    ?? throw new InvalidOperationException("Set CRMSOLID_API_KEY.");
if (!int.TryParse(Environment.GetEnvironmentVariable("CRMSOLID_TELEGRAM_ACCOUNT_ID"), out var accountId))
    throw new InvalidOperationException("Set CRMSOLID_TELEGRAM_ACCOUNT_ID to a numeric account id.");

var recipient = args[0];
var client = new CrmSolidClient(apiKey);

var request = new SendMessageRequest
{
    AccountId = accountId,
    Text = "Hello from the Pinlyx .NET SDK!",
};
if (long.TryParse(recipient, out var numericId))
    request.TelegramUserId = numericId;
else
    request.Username = recipient.TrimStart('@');

var job = await client.TelegramMessages.SendAsync(request);
Console.WriteLine($"Queued job {job.Id} (status={job.Status}).");

while (true)
{
    await Task.Delay(2000);
    var detail = await client.TelegramMessages.GetAsync(job.Id);
    Console.WriteLine($"  → {detail.Status}{(detail.LastError is null ? "" : $" — {detail.LastError}")}");
    if (detail.Status != JobStatus.Queued) break;
}
return 0;

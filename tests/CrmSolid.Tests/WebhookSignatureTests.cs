using CrmSolid.Webhooks;
using NUnit.Framework;

namespace CrmSolid.Tests;

[TestFixture]
public class WebhookSignatureTests
{
    private const string Secret = "whsec_test_secret";
    private const string Body = "{\"event\":\"contact.created\",\"id\":42}";

    [Test]
    public void Verify_AcceptsValidSignatureWithPrefix()
    {
        var sig = WebhookSignature.Compute(Secret, Body);
        Assert.That(WebhookSignature.Verify(Secret, Body, "sha256=" + sig), Is.True);
    }

    [Test]
    public void Verify_AcceptsValidSignatureWithoutPrefix()
    {
        var sig = WebhookSignature.Compute(Secret, Body);
        Assert.That(WebhookSignature.Verify(Secret, Body, sig), Is.True);
    }

    [Test]
    public void Verify_RejectsTamperedBody()
    {
        var sig = WebhookSignature.Compute(Secret, Body);
        Assert.That(WebhookSignature.Verify(Secret, Body + " ", "sha256=" + sig), Is.False);
    }

    [Test]
    public void Verify_RejectsWrongSecretOrMissingHeader()
    {
        var sig = WebhookSignature.Compute(Secret, Body);
        Assert.That(WebhookSignature.Verify("other", Body, "sha256=" + sig), Is.False);
        Assert.That(WebhookSignature.Verify(Secret, Body, null), Is.False);
    }
}

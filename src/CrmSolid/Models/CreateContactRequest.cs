namespace CrmSolid.Models;

/// <summary>
/// Request body for <c>POST /v1/contacts</c>. At least one of
/// <see cref="Name"/>, <see cref="Username"/>, or <see cref="Phone"/> is required.
/// </summary>
public sealed class CreateContactRequest
{
    /// <summary>Platform for the new contact. Defaults to <see cref="Models.Platform.Telegram"/> when omitted.</summary>
    public Platform? Platform { get; set; }

    public string? Name { get; set; }

    /// <summary>Handle with or without a leading @; the @ is stripped server-side.</summary>
    public string? Username { get; set; }

    public string? Phone { get; set; }
    public string? Notes { get; set; }
}

namespace OsitoPolarPlatform.API.Notifications.Domain.Model.ValueObjects;

/// <summary>
/// Email address value object
/// </summary>
public record EmailAddress
{
    public string Email { get; init; }
    public string? Name { get; init; }

    public EmailAddress(string email, string? name = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        Email = email.ToLowerInvariant();
        Name = name;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public override string ToString() => Name != null ? $"{Name} <{Email}>" : Email;
}

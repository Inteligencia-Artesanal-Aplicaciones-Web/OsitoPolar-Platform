namespace OsitoPolarPlatform.API.Notifications.Domain.Model.ValueObjects;

/// <summary>
/// Email template types for the notification system
/// </summary>
public enum EmailTemplate
{
    /// <summary>
    /// Welcome email sent after registration
    /// </summary>
    Welcome,

    /// <summary>
    /// Password reset email
    /// </summary>
    PasswordReset,

    /// <summary>
    /// Credential delivery email with temporary password
    /// </summary>
    CredentialDelivery,

    /// <summary>
    /// Transaction confirmation email
    /// </summary>
    TransactionConfirmation,

    /// <summary>
    /// Two-Factor Authentication setup email
    /// </summary>
    TwoFactorSetup
}

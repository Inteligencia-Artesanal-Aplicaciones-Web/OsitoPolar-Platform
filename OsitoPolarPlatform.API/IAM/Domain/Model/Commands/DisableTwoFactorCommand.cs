namespace OsitoPolarPlatform.API.IAM.Domain.Model.Commands;

/// <summary>
/// Command to disable two-factor authentication for a user
/// </summary>
/// <param name="Username">The username of the user</param>
public record DisableTwoFactorCommand(string Username);

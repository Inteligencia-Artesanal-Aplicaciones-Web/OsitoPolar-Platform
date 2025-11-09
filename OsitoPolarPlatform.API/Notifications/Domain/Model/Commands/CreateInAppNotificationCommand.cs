namespace OsitoPolarPlatform.API.Notifications.Domain.Model.Commands;

/// <summary>
/// Command to create a new in-app notification
/// </summary>
public record CreateInAppNotificationCommand(
    int UserId,
    string Title,
    string Message,
    string Type,
    string Severity = "info",
    int? EquipmentId = null,
    int? ServiceRequestId = null,
    string? Metadata = null);

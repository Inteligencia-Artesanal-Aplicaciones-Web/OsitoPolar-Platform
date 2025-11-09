namespace OsitoPolarPlatform.API.Notifications.Domain.Model.Queries;

/// <summary>
/// Query to get all notifications for a specific user
/// </summary>
public record GetNotificationsByUserIdQuery(int UserId);

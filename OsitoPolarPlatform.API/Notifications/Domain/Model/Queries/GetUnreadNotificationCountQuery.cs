namespace OsitoPolarPlatform.API.Notifications.Domain.Model.Queries;

/// <summary>
/// Query to get count of unread notifications for a user
/// </summary>
public record GetUnreadNotificationCountQuery(int UserId);

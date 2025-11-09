using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using OsitoPolarPlatform.API.IAM.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using OsitoPolarPlatform.API.Notifications.Domain.Model.Commands;
using OsitoPolarPlatform.API.Notifications.Domain.Model.Queries;
using OsitoPolarPlatform.API.Notifications.Domain.Services;

namespace OsitoPolarPlatform.API.Notifications.Interfaces.REST.Controllers;

/// <summary>
/// Controller for managing in-app notifications
/// </summary>
[ApiController]
[Route("api/v1/notifications")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("In-App Notifications Management")]
public class InAppNotificationsController : ControllerBase
{
    private readonly IInAppNotificationCommandService _commandService;
    private readonly IInAppNotificationQueryService _queryService;
    private readonly ILogger<InAppNotificationsController> _logger;

    public InAppNotificationsController(
        IInAppNotificationCommandService commandService,
        IInAppNotificationQueryService queryService,
        ILogger<InAppNotificationsController> logger)
    {
        _commandService = commandService;
        _queryService = queryService;
        _logger = logger;
    }

    /// <summary>
    /// Get all notifications for the authenticated user
    /// </summary>
    [Authorize]
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get User Notifications",
        Description = "Returns all notifications for the authenticated user, ordered by newest first.",
        OperationId = "GetUserNotifications")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of notifications")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User not authenticated")]
    public async Task<IActionResult> GetUserNotifications([FromQuery] string? userId = null)
    {
        try
        {
            var user = (User?)HttpContext.Items["User"];
            if (user == null)
                return Unauthorized(new { message = "User not authenticated" });

            var query = new GetNotificationsByUserIdQuery(user.Id);
            var notifications = await _queryService.Handle(query);

            var response = notifications.Select(n => new
            {
                id = n.Id,
                userId = n.UserId,
                title = n.Title,
                message = n.Message,
                type = n.Type,
                severity = n.Severity,
                equipmentId = n.EquipmentId,
                serviceRequestId = n.ServiceRequestId,
                isRead = n.IsRead,
                timestamp = n.Timestamp,
                readAt = n.ReadAt,
                metadata = n.Metadata
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get count of unread notifications for the authenticated user
    /// </summary>
    [Authorize]
    [HttpGet("unread-count")]
    [SwaggerOperation(
        Summary = "Get Unread Notification Count",
        Description = "Returns the count of unread notifications for the authenticated user. Perfect for displaying in a notification badge.",
        OperationId = "GetUnreadNotificationCount")]
    [SwaggerResponse(StatusCodes.Status200OK, "Unread count")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User not authenticated")]
    public async Task<IActionResult> GetUnreadNotificationCount()
    {
        try
        {
            var user = (User?)HttpContext.Items["User"];
            if (user == null)
                return Unauthorized(new { message = "User not authenticated" });

            var query = new GetUnreadNotificationCountQuery(user.Id);
            var count = await _queryService.Handle(query);

            return Ok(new { unreadCount = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notification count");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Mark a specific notification as read
    /// </summary>
    [Authorize]
    [HttpPatch("{notificationId:int}/read")]
    [SwaggerOperation(
        Summary = "Mark Notification as Read",
        Description = "Marks a specific notification as read.",
        OperationId = "MarkNotificationAsRead")]
    [SwaggerResponse(StatusCodes.Status200OK, "Notification marked as read")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User not authenticated")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Notification not found")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        try
        {
            var user = (User?)HttpContext.Items["User"];
            if (user == null)
                return Unauthorized(new { message = "User not authenticated" });

            var command = new MarkNotificationAsReadCommand(notificationId, user.Id);
            var notification = await _commandService.Handle(command);

            if (notification == null)
                return NotFound(new { message = "Notification not found" });

            return Ok(new
            {
                message = "Notification marked as read",
                notification = new
                {
                    id = notification.Id,
                    isRead = notification.IsRead,
                    readAt = notification.ReadAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Mark all notifications as read for the authenticated user
    /// </summary>
    [Authorize]
    [HttpPatch("read-all")]
    [SwaggerOperation(
        Summary = "Mark All Notifications as Read",
        Description = "Marks all notifications as read for the authenticated user.",
        OperationId = "MarkAllNotificationsAsRead")]
    [SwaggerResponse(StatusCodes.Status200OK, "All notifications marked as read")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User not authenticated")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var user = (User?)HttpContext.Items["User"];
            if (user == null)
                return Unauthorized(new { message = "User not authenticated" });

            var command = new MarkAllNotificationsAsReadCommand(user.Id);
            await _commandService.Handle(command);

            return Ok(new { message = "All notifications marked as read" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return BadRequest(new { message = ex.Message });
        }
    }
}

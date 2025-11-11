using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Repositories;
using OsitoPolarPlatform.API.Notifications.Interfaces.ACL;

namespace OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Controllers;

/// <summary>
/// Controller for maintenance forecast and reminder notifications
/// </summary>
[ApiController]
[Route("api/v1/maintenance-notifications")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Maintenance Forecast Notifications")]
public class MaintenanceNotificationsController : ControllerBase
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly INotificationContextFacade _notificationFacade;
    private readonly ILogger<MaintenanceNotificationsController> _logger;

    // Default maintenance interval in days (can be customized per equipment type later)
    private const int DEFAULT_MAINTENANCE_INTERVAL_DAYS = 90; // 3 months
    private const int REMINDER_THRESHOLD_DAYS = 7; // Send reminder 7 days before maintenance due

    public MaintenanceNotificationsController(
        IEquipmentRepository equipmentRepository,
        INotificationContextFacade notificationFacade,
        ILogger<MaintenanceNotificationsController> logger)
    {
        _equipmentRepository = equipmentRepository;
        _notificationFacade = notificationFacade;
        _logger = logger;
    }

    /// <summary>
    /// Check all equipment and send maintenance reminder notifications
    /// </summary>
    /// <remarks>
    /// This endpoint checks all active equipment and sends notifications to owners
    /// when maintenance is due within the next 7 days.
    ///
    /// This can be called:
    /// - Manually via API
    /// - By a scheduled job/cron
    /// - By a frontend scheduler
    /// </remarks>
    [HttpPost("check-and-notify")]
    [SwaggerOperation(
        Summary = "Check Equipment Maintenance and Send Reminders",
        Description = "Checks all equipment for upcoming maintenance and sends notification reminders to owners",
        OperationId = "CheckAndNotifyMaintenance")]
    [SwaggerResponse(StatusCodes.Status200OK, "Maintenance check completed")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Error during maintenance check")]
    public async Task<IActionResult> CheckAndNotifyMaintenance()
    {
        try
        {
            _logger.LogInformation("Starting maintenance forecast check at {Time}", DateTime.UtcNow);

            // Get all active equipment
            var allEquipment = await _equipmentRepository.ListAsync();
            var activeEquipment = allEquipment
                .Where(e => e.Status == Domain.Model.ValueObjects.EEquipmentStatus.Active)
                .ToList();

            _logger.LogInformation("Found {Count} active equipment to check", activeEquipment.Count);

            var notificationsSent = 0;
            var equipmentChecked = 0;

            foreach (var equipment in activeEquipment)
            {
                equipmentChecked++;

                // Calculate last maintenance date (use installation date as fallback)
                var lastMaintenanceDate = equipment.InstallationDate.DateTime;

                // Calculate next maintenance due date
                var nextMaintenanceDue = lastMaintenanceDate.AddDays(DEFAULT_MAINTENANCE_INTERVAL_DAYS);

                // Calculate days until maintenance
                var daysUntilMaintenance = (int)(nextMaintenanceDue - DateTime.UtcNow).TotalDays;

                _logger.LogDebug("Equipment {EquipmentId} ({Name}): Last maintenance {LastDate}, Next due {NextDate}, Days until: {Days}",
                    equipment.Id, equipment.Name, lastMaintenanceDate, nextMaintenanceDue, daysUntilMaintenance);

                // Send notification if maintenance is due within threshold
                if (daysUntilMaintenance <= REMINDER_THRESHOLD_DAYS && daysUntilMaintenance >= 0)
                {
                    var message = daysUntilMaintenance == 0
                        ? $"Maintenance overdue for {equipment.Name}"
                        : $"Maintenance due in {daysUntilMaintenance} days for {equipment.Name}";
                    await _notificationFacade.CreateInAppNotification(
                        equipment.OwnerId,
                        "⚠️ Maintenance Reminder",
                        message);

                    notificationsSent++;

                    _logger.LogInformation(
                        "Sent maintenance reminder for equipment {EquipmentId} ({Name}) to owner {OwnerId}. Due in {Days} days",
                        equipment.Id, equipment.Name, equipment.OwnerId, daysUntilMaintenance);
                }
                else if (daysUntilMaintenance < 0)
                {
                    _logger.LogWarning(
                        "Equipment {EquipmentId} ({Name}) is OVERDUE for maintenance by {Days} days",
                        equipment.Id, equipment.Name, Math.Abs(daysUntilMaintenance));

                    // Send urgent notification for overdue maintenance
                    var message = $"Maintenance overdue for {equipment.Name}";
                    await _notificationFacade.CreateInAppNotification(
                        equipment.OwnerId,
                        "⚠️ Maintenance Reminder",
                        message);

                    notificationsSent++;
                }
            }

            _logger.LogInformation(
                "Maintenance check completed. Checked {Checked} equipment, sent {Sent} notifications",
                equipmentChecked, notificationsSent);

            return Ok(new
            {
                success = true,
                message = "Maintenance forecast check completed",
                equipmentChecked,
                notificationsSent,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during maintenance forecast check");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = $"Error during maintenance check: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Get maintenance forecast for specific equipment
    /// </summary>
    [HttpGet("forecast/{equipmentId:int}")]
    [SwaggerOperation(
        Summary = "Get Maintenance Forecast for Equipment",
        Description = "Returns maintenance forecast information for a specific equipment",
        OperationId = "GetMaintenanceForecast")]
    [SwaggerResponse(StatusCodes.Status200OK, "Forecast retrieved")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    public async Task<IActionResult> GetMaintenanceForecast(int equipmentId)
    {
        try
        {
            var equipment = await _equipmentRepository.FindByIdAsync(equipmentId);
            if (equipment == null)
                return NotFound(new { message = "Equipment not found" });

            // Calculate maintenance dates
            var lastMaintenanceDate = equipment.InstallationDate.DateTime;
            var nextMaintenanceDue = lastMaintenanceDate.AddDays(DEFAULT_MAINTENANCE_INTERVAL_DAYS);
            var daysUntilMaintenance = (int)(nextMaintenanceDue - DateTime.UtcNow).TotalDays;

            var status = daysUntilMaintenance < 0 ? "overdue" :
                         daysUntilMaintenance <= REMINDER_THRESHOLD_DAYS ? "due_soon" : "ok";

            return Ok(new
            {
                equipmentId = equipment.Id,
                equipmentName = equipment.Name,
                lastMaintenanceDate,
                nextMaintenanceDue,
                daysUntilMaintenance,
                status,
                maintenanceIntervalDays = DEFAULT_MAINTENANCE_INTERVAL_DAYS,
                reminderThresholdDays = REMINDER_THRESHOLD_DAYS
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting maintenance forecast for equipment {EquipmentId}", equipmentId);
            return BadRequest(new { message = ex.Message });
        }
    }
}

using OsitoPolarPlatform.API.Notifications.Domain.Model.Commands;
using OsitoPolarPlatform.API.Notifications.Domain.Services;

namespace OsitoPolarPlatform.API.Notifications.Application.Internal.CommandServices;

/// <summary>
/// Service to generate notifications automatically based on system events
/// This is used by other bounded contexts to create notifications
/// </summary>
public class NotificationGeneratorService
{
    private readonly IInAppNotificationCommandService _notificationCommandService;
    private readonly ILogger<NotificationGeneratorService> _logger;

    public NotificationGeneratorService(
        IInAppNotificationCommandService notificationCommandService,
        ILogger<NotificationGeneratorService> logger)
    {
        _notificationCommandService = notificationCommandService;
        _logger = logger;
    }

    /// <summary>
    /// Generate notification when equipment anomaly is detected
    /// </summary>
    public async Task NotifyEquipmentAnomaly(
        int ownerId,
        int equipmentId,
        string equipmentName,
        string anomalyType,
        string details)
    {
        try
        {
            _logger.LogInformation(
                "Generating equipment anomaly notification for owner {OwnerId}, equipment {EquipmentId}",
                ownerId, equipmentId);

            var command = new CreateInAppNotificationCommand(
                UserId: ownerId,
                Title: "⚠️ Equipment Alert",
                Message: $"Your {equipmentName} has detected an anomaly: {anomalyType}. {details}",
                Type: "equipment_alert",
                Severity: "warning",
                EquipmentId: equipmentId
            );

            await _notificationCommandService.Handle(command);
            _logger.LogInformation("Equipment anomaly notification created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating equipment anomaly notification");
        }
    }

    /// <summary>
    /// Generate notification when a provider accepts a service request
    /// </summary>
    public async Task NotifyServiceRequestAccepted(
        int ownerId,
        int serviceRequestId,
        string providerName,
        string serviceTitle)
    {
        try
        {
            _logger.LogInformation(
                "Generating service accepted notification for owner {OwnerId}, service {ServiceRequestId}",
                ownerId, serviceRequestId);

            var command = new CreateInAppNotificationCommand(
                UserId: ownerId,
                Title: "✅ Service Request Accepted",
                Message: $"{providerName} has accepted your service request: {serviceTitle}",
                Type: "service_accepted",
                Severity: "success",
                ServiceRequestId: serviceRequestId
            );

            await _notificationCommandService.Handle(command);
            _logger.LogInformation("Service accepted notification created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service accepted notification");
        }
    }

    /// <summary>
    /// Generate notification when a technician is assigned
    /// </summary>
    public async Task NotifyTechnicianAssigned(
        int ownerId,
        int serviceRequestId,
        string technicianName,
        string serviceTitle)
    {
        try
        {
            _logger.LogInformation(
                "Generating technician assigned notification for owner {OwnerId}, service {ServiceRequestId}",
                ownerId, serviceRequestId);

            var command = new CreateInAppNotificationCommand(
                UserId: ownerId,
                Title: "👷 Technician Assigned",
                Message: $"Technician {technicianName} has been assigned to your service: {serviceTitle}",
                Type: "technician_assigned",
                Severity: "info",
                ServiceRequestId: serviceRequestId
            );

            await _notificationCommandService.Handle(command);
            _logger.LogInformation("Technician assigned notification created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating technician assigned notification");
        }
    }

    /// <summary>
    /// Generate notification when service is completed
    /// </summary>
    public async Task NotifyServiceCompleted(
        int ownerId,
        int serviceRequestId,
        string serviceTitle,
        decimal? cost)
    {
        try
        {
            _logger.LogInformation(
                "Generating service completed notification for owner {OwnerId}, service {ServiceRequestId}",
                ownerId, serviceRequestId);

            var costMessage = cost.HasValue ? $" Cost: ${cost.Value:F2}" : "";

            var command = new CreateInAppNotificationCommand(
                UserId: ownerId,
                Title: "✅ Service Completed",
                Message: $"Your service request has been completed: {serviceTitle}.{costMessage}",
                Type: "service_completed",
                Severity: "success",
                ServiceRequestId: serviceRequestId
            );

            await _notificationCommandService.Handle(command);
            _logger.LogInformation("Service completed notification created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service completed notification");
        }
    }

    /// <summary>
    /// Generate notification for provider when payment is received
    /// </summary>
    public async Task NotifyPaymentReceived(
        int providerId,
        int serviceRequestId,
        decimal amount,
        string serviceTitle)
    {
        try
        {
            _logger.LogInformation(
                "Generating payment received notification for provider {ProviderId}, service {ServiceRequestId}",
                providerId, serviceRequestId);

            var command = new CreateInAppNotificationCommand(
                UserId: providerId,
                Title: "💰 Payment Received",
                Message: $"You earned ${amount:F2} from service: {serviceTitle}",
                Type: "payment_received",
                Severity: "success",
                ServiceRequestId: serviceRequestId
            );

            await _notificationCommandService.Handle(command);
            _logger.LogInformation("Payment received notification created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment received notification");
        }
    }

    /// <summary>
    /// Generate notification for maintenance reminder
    /// </summary>
    public async Task NotifyMaintenanceReminder(
        int ownerId,
        int equipmentId,
        string equipmentName,
        int daysUntilMaintenance)
    {
        try
        {
            _logger.LogInformation(
                "Generating maintenance reminder for owner {OwnerId}, equipment {EquipmentId}",
                ownerId, equipmentId);

            var command = new CreateInAppNotificationCommand(
                UserId: ownerId,
                Title: "🔧 Maintenance Reminder",
                Message: $"Your {equipmentName} needs maintenance in {daysUntilMaintenance} days",
                Type: "maintenance_reminder",
                Severity: "info",
                EquipmentId: equipmentId
            );

            await _notificationCommandService.Handle(command);
            _logger.LogInformation("Maintenance reminder notification created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating maintenance reminder notification");
        }
    }

    /// <summary>
    /// Generate notification when new service request is available (for providers)
    /// </summary>
    public async Task NotifyNewServiceRequestAvailable(
        int providerId,
        int serviceRequestId,
        string serviceTitle,
        string location)
    {
        try
        {
            _logger.LogInformation(
                "Generating new service request notification for provider {ProviderId}",
                providerId);

            var command = new CreateInAppNotificationCommand(
                UserId: providerId,
                Title: "🔔 New Service Request",
                Message: $"New service request available: {serviceTitle} at {location}",
                Type: "service_available",
                Severity: "info",
                ServiceRequestId: serviceRequestId
            );

            await _notificationCommandService.Handle(command);
            _logger.LogInformation("New service request notification created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new service request notification");
        }
    }
}

using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects; // For EPriority, EServiceType

namespace OsitoPolarPlatform.API.WorkOrders.Domain.Model.Commands;

/// <summary>
/// Command to create a new WorkOrder.
/// Supports creation from a ServiceRequest or manual creation.
/// </summary>
public record CreateWorkOrderCommand(
    string Title,
    string Description,
    string IssueDetails,
    int EquipmentId,
    EServiceType ServiceType,
    string ServiceAddress,
    EPriority Priority,
    int? ServiceRequestId = null, 
    int? ReportedByUserId = null, 
    EUrgency? Urgency = null, 
    bool? IsEmergency = null,
    DateTimeOffset? ScheduledDate = null,
    string TimeSlot = ""
);
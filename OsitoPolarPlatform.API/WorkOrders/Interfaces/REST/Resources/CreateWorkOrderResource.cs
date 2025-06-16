using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Resources;

/// <summary>
/// Resource for creating a new WorkOrder.
/// Designed to support both manual and ServiceRequest-driven creation.
/// </summary>
public record CreateWorkOrderResource(
    string Title,
    string Description,
    string IssueDetails,
    int EquipmentId,
    EServiceType ServiceType,
    string ServiceAddress,
    EPriority Priority,
    int? ServiceRequestId = null,
    DateTimeOffset? ScheduledDate = null,
    string? TimeSlot = null
);
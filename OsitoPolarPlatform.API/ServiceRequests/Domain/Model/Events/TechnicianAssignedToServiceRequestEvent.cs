using OsitoPolarPlatform.API.Shared.Domain.Model.Events;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Events;

/// <summary>
/// Domain event raised when a technician is assigned to a service request
/// This triggers the creation of a Work Order in the WorkOrders bounded context
/// </summary>
public record TechnicianAssignedToServiceRequestEvent : IEvent
{
    public int ServiceRequestId { get; init; }
    public int TechnicianId { get; init; }
    public int EquipmentId { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public string IssueDetails { get; init; }
    public EServiceType ServiceType { get; init; }
    public EPriority Priority { get; init; }
    public DateTimeOffset? ScheduledDate { get; init; }
    public string? TimeSlot { get; init; }
    public string? ServiceAddress { get; init; }
    public DateTime OccurredAt { get; init; }

    public TechnicianAssignedToServiceRequestEvent(
        int serviceRequestId,
        int technicianId,
        int equipmentId,
        string title,
        string description,
        string issueDetails,
        EServiceType serviceType,
        EPriority priority,
        DateTimeOffset? scheduledDate,
        string? timeSlot,
        string? serviceAddress)
    {
        ServiceRequestId = serviceRequestId;
        TechnicianId = technicianId;
        EquipmentId = equipmentId;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        IssueDetails = issueDetails ?? throw new ArgumentNullException(nameof(issueDetails));
        ServiceType = serviceType;
        Priority = priority;
        ScheduledDate = scheduledDate;
        TimeSlot = timeSlot;
        ServiceAddress = serviceAddress;
        OccurredAt = DateTime.UtcNow;
    }
}

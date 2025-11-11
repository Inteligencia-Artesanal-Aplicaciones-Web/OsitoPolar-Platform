using OsitoPolarPlatform.API.Shared.Domain.Model.Events;

namespace OsitoPolarPlatform.API.WorkOrders.Domain.Model.Events;

/// <summary>
/// Domain event raised when a WorkOrder status changes (completed, cancelled, etc.)
/// </summary>
public record WorkOrderStatusChangedEvent : IEvent
{
    public int WorkOrderId { get; init; }
    public int ServiceRequestId { get; init; }
    public string NewStatus { get; init; }
    public string? ResolutionDetails { get; init; }
    public DateTime OccurredAt { get; init; }

    public WorkOrderStatusChangedEvent(
        int workOrderId,
        int serviceRequestId,
        string newStatus,
        string? resolutionDetails = null)
    {
        WorkOrderId = workOrderId;
        ServiceRequestId = serviceRequestId;
        NewStatus = newStatus ?? throw new ArgumentNullException(nameof(newStatus));
        ResolutionDetails = resolutionDetails;
        OccurredAt = DateTime.UtcNow;
    }
}

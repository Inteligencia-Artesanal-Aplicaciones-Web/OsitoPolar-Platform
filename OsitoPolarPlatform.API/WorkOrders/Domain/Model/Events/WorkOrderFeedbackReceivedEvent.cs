using OsitoPolarPlatform.API.Shared.Domain.Model.Events;

namespace OsitoPolarPlatform.API.WorkOrders.Domain.Model.Events;

/// <summary>
/// Domain event raised when a WorkOrder receives customer feedback
/// This should update the related ServiceRequest
/// </summary>
public record WorkOrderFeedbackReceivedEvent : IEvent
{
    public int WorkOrderId { get; init; }
    public int ServiceRequestId { get; init; }
    public int Rating { get; init; }
    public DateTime OccurredAt { get; init; }

    public WorkOrderFeedbackReceivedEvent(
        int workOrderId,
        int serviceRequestId,
        int rating)
    {
        WorkOrderId = workOrderId;
        ServiceRequestId = serviceRequestId;
        Rating = rating;
        OccurredAt = DateTime.UtcNow;
    }
}

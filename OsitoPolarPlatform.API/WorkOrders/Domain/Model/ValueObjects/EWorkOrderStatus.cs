namespace OsitoPolarPlatform.API.WorkOrders.Domain.Model.ValueObjects;

/// <summary>
/// Represents the different statuses an active Work Order can have.
/// </summary>
public enum EWorkOrderStatus
{
    Created,
    Assigned,
    InProgress,
    OnHold,
    Completed,
    Resolved,
    Cancelled
}
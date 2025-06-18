namespace OsitoPolarPlatform.API.WorkOrders.Domain.Model.Commands;

/// <summary>
/// Command to add resolution details to a completed WorkOrder.
/// </summary>
public record AddWorkOrderResolutionDetailsCommand(
    int WorkOrderId,
    string ResolutionDetails,
    string TechnicianNotes,
    decimal? Cost
);
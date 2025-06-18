namespace OsitoPolarPlatform.API.WorkOrders.Domain.Model.Commands;

/// <summary>
/// Command to add customer feedback (rating and comment) to a WorkOrder.
/// </summary>
public record AddWorkOrderCustomerFeedbackCommand(
    int WorkOrderId,
    int Rating
    );
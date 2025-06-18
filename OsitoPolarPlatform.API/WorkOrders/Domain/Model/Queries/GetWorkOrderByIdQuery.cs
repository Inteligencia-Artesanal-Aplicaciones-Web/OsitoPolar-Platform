namespace OsitoPolarPlatform.API.WorkOrders.Domain.Model.Queries;

/// <summary>
/// Represents a query to retrieve a Work Order by its unique identifier.
/// </summary>
public record GetWorkOrderByIdQuery(int WorkOrderId);
namespace OsitoPolarPlatform.API.WorkOrders.Domain.Model.Queries;

/// <summary>
/// Represents a query to retrieve a Work Order by its Work Order Number.
/// </summary>
public record GetWorkOrderByWorkOrderNumberQuery(string WorkOrderNumber);
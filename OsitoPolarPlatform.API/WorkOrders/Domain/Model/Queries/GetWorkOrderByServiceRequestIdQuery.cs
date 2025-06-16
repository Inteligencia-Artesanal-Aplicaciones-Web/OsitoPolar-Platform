namespace OsitoPolarPlatform.API.WorkOrders.Domain.Model.Queries;

/// <summary>
/// Represents a query to retrieve a Work Order by its associated Service Request ID.
/// </summary>
public record GetWorkOrderByServiceRequestIdQuery(int ServiceRequestId);
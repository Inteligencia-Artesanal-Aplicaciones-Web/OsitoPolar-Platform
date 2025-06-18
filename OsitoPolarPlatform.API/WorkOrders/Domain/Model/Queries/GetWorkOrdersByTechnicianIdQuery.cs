namespace OsitoPolarPlatform.API.WorkOrders.Domain.Model.Queries;

/// <summary>
/// Represents a query to retrieve Work Orders by the assigned technician ID.
/// </summary>
public record GetWorkOrdersByTechnicianIdQuery(int TechnicianId);
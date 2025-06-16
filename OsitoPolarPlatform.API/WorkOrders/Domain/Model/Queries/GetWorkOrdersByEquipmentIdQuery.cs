namespace OsitoPolarPlatform.API.WorkOrders.Domain.Model.Queries;

/// <summary>
/// Represents a query to retrieve Work Orders by the associated equipment ID.
/// </summary>
public record GetWorkOrdersByEquipmentIdQuery(int EquipmentId);
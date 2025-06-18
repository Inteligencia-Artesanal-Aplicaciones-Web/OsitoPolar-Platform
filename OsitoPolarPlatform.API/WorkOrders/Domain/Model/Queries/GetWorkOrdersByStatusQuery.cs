using OsitoPolarPlatform.API.WorkOrders.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.WorkOrders.Domain.Model.Queries;

/// <summary>
/// Represents a query to retrieve Work Orders by their status.
/// </summary>
public record GetWorkOrdersByStatusQuery(EWorkOrderStatus Status);
namespace OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Resources;

/// <summary>
/// Resource for assigning a technician to a WorkOrder.
/// </summary>
public record AssignTechnicianResource(
    int TechnicianId
);
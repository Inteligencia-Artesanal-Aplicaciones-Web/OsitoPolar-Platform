namespace OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Resources;

public record TechnicianResource(
    int Id,
    string Name,
    string Specialization,
    string Phone,
    string Email,
    decimal AverageRating,
    string Availability,
    int CompanyId
);

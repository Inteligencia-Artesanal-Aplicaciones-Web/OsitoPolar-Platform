namespace OsitoPolarPlatform.API.bc_technicians.Interfaces.REST.Resources;
/*
public record CreateTechnicianResource(
    string Name,
    string Specialization,
    string Phone,
    string Email,
    string Availability,
    int CompanyId
);
*/

public class CreateTechnicianResource
{
    public string Name { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Availability { get; set; } = string.Empty;
    public int CompanyId { get; set; }

}

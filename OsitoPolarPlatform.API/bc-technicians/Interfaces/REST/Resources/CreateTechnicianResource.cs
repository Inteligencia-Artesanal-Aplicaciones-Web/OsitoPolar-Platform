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
    public string Name { get; set; }
    public string Specialization { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Availability { get; set; }
    public int CompanyId { get; set; }

}

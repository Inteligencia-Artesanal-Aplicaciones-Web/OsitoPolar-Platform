namespace OsitoPolarPlatform.API.bc_technicians.Domain.Model.Commands;

public record CreateTechnicianCommand(
    string Name,
    string Specialization,
    string Phone,
    string Email,
    int Rating,
    string Availability,
    int CompanyId
);
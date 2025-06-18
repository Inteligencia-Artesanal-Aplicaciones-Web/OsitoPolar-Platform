namespace OsitoPolarPlatform.API.bc_technicians.Domain.Model.Commands;

public record CreateTechnicianCommand(
    string Name,
    string Specialization,
    string Phone,
    string Email,
    string Availability,
    int CompanyId
);
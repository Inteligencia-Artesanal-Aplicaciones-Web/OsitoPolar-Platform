using OsitoPolarPlatform.API.bc_technicians.Domain.Model.Commands;
using OsitoPolarPlatform.API.bc_technicians.Domain.Model.Entities;

namespace OsitoPolarPlatform.API.bc_technicians.Domain.Services;

public interface ITechnicianCommandService
{
    Task<Technician?> Handle(CreateTechnicianCommand command);
}
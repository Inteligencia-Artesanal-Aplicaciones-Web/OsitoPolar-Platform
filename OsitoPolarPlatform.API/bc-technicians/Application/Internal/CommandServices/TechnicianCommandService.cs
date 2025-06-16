using OsitoPolarPlatform.API.bc_technicians.Domain.Model.Commands;
using OsitoPolarPlatform.API.bc_technicians.Domain.Model.Entities;
using OsitoPolarPlatform.API.bc_technicians.Domain.Repositories;
using OsitoPolarPlatform.API.bc_technicians.Domain.Services;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;

namespace OsitoPolarPlatform.API.bc_technicians.Application.Internal.CommandServices;

public class TechnicianCommandService(ITechnicianRepository technicianRepository, IUnitOfWork unitOfWork ) 
    : ITechnicianCommandService
{

    public async Task<Technician?> Handle(CreateTechnicianCommand command)
    {
        var technician = new Technician(command);
        await technicianRepository.AddAsync(technician);
        await unitOfWork.CompleteAsync();
        return technician;
    }
}

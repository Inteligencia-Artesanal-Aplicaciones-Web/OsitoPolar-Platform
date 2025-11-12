using Cortex.Mediator;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Events;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Commands;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Entities;
using OsitoPolarPlatform.API.WorkOrders.Domain.Repositories;
using OsitoPolarPlatform.API.WorkOrders.Domain.Services;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;

namespace OsitoPolarPlatform.API.WorkOrders.Application.Internal.CommandServices;

public class TechnicianCommandService(ITechnicianRepository technicianRepository, IUnitOfWork unitOfWork, IMediator domainEventPublisher)
    : ITechnicianCommandService
{

    public async Task<Technician?> Handle(CreateTechnicianCommand command)
    {
        var technician = new Technician(command);
        await technicianRepository.AddAsync(technician);
        await unitOfWork.CompleteAsync();

        await domainEventPublisher.PublishAsync(new TechnicianCreatedEvent(technician.Name, technician.Specialization, technician.Phone, technician.Email, technician.Availability, technician.CompanyId));
        return technician;
    }
}

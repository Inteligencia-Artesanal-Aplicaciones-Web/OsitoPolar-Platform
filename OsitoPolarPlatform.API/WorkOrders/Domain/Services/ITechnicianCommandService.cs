using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Commands;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Entities;

namespace OsitoPolarPlatform.API.WorkOrders.Domain.Services;

public interface ITechnicianCommandService
{
    Task<Technician?> Handle(CreateTechnicianCommand command);
}

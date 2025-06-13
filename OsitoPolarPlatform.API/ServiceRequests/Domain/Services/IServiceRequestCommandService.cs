using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Aggregates.ACME.PolarBear.Platform.API.Services.Domain.Model.Entities;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Commands;

namespace OsitoPolarPlatform.API.ServiceRequests.Domain.Services;


public interface IServiceRequestCommandService
{
    Task<ServiceRequest?> Handle(CreateServiceRequestCommand command);
    Task<ServiceRequest?> Handle(UpdateServiceRequestCommand command);
    Task<ServiceRequest?> Handle(AssignTechnicianToServiceRequestCommand command);
    Task<ServiceRequest?> Handle(UpdateServiceRequestStatusCommand command);
    Task<ServiceRequest?> Handle(AddCustomerFeedbackToServiceRequestCommand command);
}
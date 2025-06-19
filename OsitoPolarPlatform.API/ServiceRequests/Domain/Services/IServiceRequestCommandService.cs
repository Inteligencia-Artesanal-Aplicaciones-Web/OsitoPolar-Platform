using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Commands;

namespace OsitoPolarPlatform.API.ServiceRequests.Domain.Services;


public interface IServiceRequestCommandService
{
    Task<ServiceRequest?> Handle(CreateServiceRequestCommand command);
    Task<ServiceRequest?> Handle(UpdateServiceRequestCommand command);
    Task<ServiceRequest?> Handle(AssignTechnicianToServiceRequestCommand command);
    Task<ServiceRequest?> Handle(AddCustomerFeedbackToServiceRequestCommand command);
    Task<ServiceRequest?> Handle(RejectServiceRequestCommand command); 
    Task<ServiceRequest?> Handle(CancelServiceRequestCommand command); 
}
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Repositories;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Services;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Commands;
using OsitoPolarPlatform.API.WorkOrders.Domain.Repositories;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Aggregates;

namespace OsitoPolarPlatform.API.ServiceRequests.Application.Internal.CommandServices;

public class ServiceRequestCommandService(
    IServiceRequestRepository serviceRequestRepository,
    IWorkOrderRepository workOrderRepository,
    IUnitOfWork unitOfWork) : IServiceRequestCommandService
{
    public async Task<ServiceRequest?> Handle(CreateServiceRequestCommand command)
    {
        var serviceRequest = new ServiceRequest(
            command.Title,
            command.Description,
            command.IssueDetails,
            command.ClientId, 
            command.CompanyId,
            command.EquipmentId,
            command.ServiceType,
            command.Priority,
            command.Urgency,
            command.IsEmergency,
            command.ScheduledDate,
            command.TimeSlot,
            command.ServiceAddress
        );

        await serviceRequestRepository.AddAsync(serviceRequest);
        await unitOfWork.CompleteAsync();

        return serviceRequest;
    }
    
    
    public async Task<ServiceRequest?> Handle(UpdateServiceRequestCommand command)
    {
        var serviceRequest = await serviceRequestRepository.FindByIdAsync(command.Id);
        if (serviceRequest is null) return null; 

        serviceRequest.UpdateStatus(command.Status);
        serviceRequest.AssignTechnician(command.AssignedTechnicianId ?? 0); 
        
        if (command.AssignedTechnicianId.HasValue)
        {
            serviceRequest.AssignTechnician(command.AssignedTechnicianId.Value);
        }
        
        // serviceRequest.UpdateTitle(command.Title);
        // serviceRequest.UpdateDescription(command.Description);
        // serviceRequest.UpdateIssueDetails(command.IssueDetails);
        // serviceRequest.UpdatePriority(command.Priority);
        // serviceRequest.UpdateUrgency(command.Urgency);
        // serviceRequest.UpdateIsEmergency(command.IsEmergency);
        // serviceRequest.UpdateServiceType(command.ServiceType);
        // serviceRequest.UpdateScheduledDate(command.ScheduledDate);
        // serviceRequest.UpdateTimeSlot(command.TimeSlot);
        // serviceRequest.UpdateServiceAddress(command.ServiceAddress);


        await unitOfWork.CompleteAsync();
        return serviceRequest;
    }

    public async Task<ServiceRequest?> Handle(AssignTechnicianToServiceRequestCommand command)
    {
        var serviceRequest = await serviceRequestRepository.FindByIdAsync(command.ServiceRequestId);
        if (serviceRequest == null) return null;

        serviceRequest.AssignTechnician(command.TechnicianId);
        serviceRequestRepository.Update(serviceRequest); 

        var workOrder = new WorkOrder(
            serviceRequest.Id,
            serviceRequest.Title,
            serviceRequest.Description,
            serviceRequest.IssueDetails,
           // command.ClientId,
           // command.CompanyId,
            serviceRequest.EquipmentId,
            serviceRequest.ServiceType,
            serviceRequest.Priority,
            serviceRequest.ScheduledDate,
            serviceRequest.TimeSlot,
            serviceRequest.ServiceAddress
        );
        
        workOrder.AssignTechnician(command.TechnicianId);
        await workOrderRepository.AddAsync(workOrder);
        await unitOfWork.CompleteAsync(); 

        return serviceRequest;
    }

    public async Task<ServiceRequest?> Handle(AddCustomerFeedbackToServiceRequestCommand command)
    {
        var serviceRequest = await serviceRequestRepository.FindByIdAsync(command.ServiceRequestId);
        if (serviceRequest == null) return null;

        serviceRequest.AddCustomerFeedback(command.Rating);

        var workOrder = await workOrderRepository.FindByServiceRequestIdAsync(serviceRequest.Id);
        if (workOrder != null)
        {
            workOrder.SetCustomerFeedbackRating(command.Rating);
            workOrderRepository.Update(workOrder); 
        }

        serviceRequestRepository.Update(serviceRequest); 
        await unitOfWork.CompleteAsync();

        return serviceRequest;
    }

    public async Task<ServiceRequest?> Handle(RejectServiceRequestCommand command)
    {
        var serviceRequest = await serviceRequestRepository.FindByIdAsync(command.ServiceRequestId);
        if (serviceRequest == null) return null;

        serviceRequest.Reject();
        serviceRequestRepository.Update(serviceRequest); 
        await unitOfWork.CompleteAsync();

        return serviceRequest;
    }

    public async Task<ServiceRequest?> Handle(CancelServiceRequestCommand command)
    {
        var serviceRequest = await serviceRequestRepository.FindByIdAsync(command.ServiceRequestId);
        if (serviceRequest == null) return null;

        serviceRequest.Cancel();
        serviceRequestRepository.Update(serviceRequest); 
        await unitOfWork.CompleteAsync();

        return serviceRequest;
    }
}
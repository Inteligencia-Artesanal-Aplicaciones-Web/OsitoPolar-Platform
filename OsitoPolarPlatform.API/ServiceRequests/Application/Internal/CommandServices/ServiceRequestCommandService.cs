using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Commands;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Repositories;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Services;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;

namespace OsitoPolarPlatform.API.ServiceRequests.Application.Internal.CommandServices;


public class ServiceRequestCommandService(
    IServiceRequestRepository serviceRequestRepository,
    IUnitOfWork unitOfWork) : IServiceRequestCommandService
{
    public async Task<ServiceRequest?> Handle(CreateServiceRequestCommand command)
    {
       
        //if (await serviceRequestRepository.ExistsByOrderNumberAsync(command.OrderNumber))
        //   throw new ArgumentException("Service request with this order number already exists.");
       
        var serviceRequest = new ServiceRequest(
            command.Title,
            command.Description,
            command.IssueDetails,
            command.EquipmentId,
            command.ServiceType, 
            command.ReportedByUserId,
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
        if (serviceRequest is null) return null;

        serviceRequest.AssignTechnician(command.TechnicianId);
        await unitOfWork.CompleteAsync();
        return serviceRequest;
    }

    public async Task<ServiceRequest?> Handle(UpdateServiceRequestStatusCommand command)
    {
        var serviceRequest = await serviceRequestRepository.FindByIdAsync(command.ServiceRequestId);
        if (serviceRequest is null) return null;

        serviceRequest.UpdateStatus(command.NewStatus);
        await unitOfWork.CompleteAsync();
        return serviceRequest;
    }

    //public async Task<ServiceRequest?> Handle(AddResolutionDetailsToServiceRequestCommand command)
    //{
    //    var serviceRequest = await serviceRequestRepository.FindByIdAsync(command.ServiceRequestId);
    //    if (serviceRequest is null) return null;
//
    //    serviceRequest.AddResolutionDetails(command.ResolutionDetails, command.TechnicianNotes, command.Cost);
    //    await unitOfWork.CompleteAsync();
    //    return serviceRequest;
    //}

    public async Task<ServiceRequest?> Handle(AddCustomerFeedbackToServiceRequestCommand command)
    {
        var serviceRequest = await serviceRequestRepository.FindByIdAsync(command.ServiceRequestId);
        if (serviceRequest is null) return null;

        serviceRequest.AddCustomerFeedback(command.Rating);
        await unitOfWork.CompleteAsync();
        return serviceRequest;
    }
}


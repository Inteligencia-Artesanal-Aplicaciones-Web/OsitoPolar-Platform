using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Repositories;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Services;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Commands;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Events;
using OsitoPolarPlatform.API.Shared.Domain.Services;
using OsitoPolarPlatform.API.Notifications.Interfaces.ACL;
using OsitoPolarPlatform.API.Profiles.Interfaces.ACL;
using OsitoPolarPlatform.API.EquipmentManagement.Interfaces.ACL;

namespace OsitoPolarPlatform.API.ServiceRequests.Application.Internal.CommandServices;

public class ServiceRequestCommandService : IServiceRequestCommandService
{
    private readonly IServiceRequestRepository serviceRequestRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IEventBus eventBus;
    private readonly INotificationContextFacade notificationFacade;
    private readonly IProfilesContextFacade profilesFacade;
    private readonly IEquipmentContextFacade equipmentFacade;

    public ServiceRequestCommandService(
        IServiceRequestRepository serviceRequestRepository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        INotificationContextFacade notificationFacade,
        IProfilesContextFacade profilesFacade,
        IEquipmentContextFacade equipmentFacade)
    {
        this.serviceRequestRepository = serviceRequestRepository;
        this.unitOfWork = unitOfWork;
        this.eventBus = eventBus;
        this.notificationFacade = notificationFacade;
        this.profilesFacade = profilesFacade;
        this.equipmentFacade = equipmentFacade;
    }
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
        await unitOfWork.CompleteAsync();

        // Publish domain event instead of creating WorkOrder directly
        // This maintains separation between ServiceRequests and WorkOrders bounded contexts
        var technicianAssignedEvent = new TechnicianAssignedToServiceRequestEvent(
            serviceRequest.Id,
            command.TechnicianId,
            serviceRequest.EquipmentId,
            serviceRequest.Title,
            serviceRequest.Description,
            serviceRequest.IssueDetails,
            serviceRequest.ServiceType,
            serviceRequest.Priority,
            serviceRequest.ScheduledDate,
            serviceRequest.TimeSlot,
            serviceRequest.ServiceAddress
        );

        await eventBus.PublishAsync(technicianAssignedEvent);

        Console.WriteLine($"[ServiceRequests BC] Published TechnicianAssignedToServiceRequestEvent for SR {serviceRequest.Id}");

        return serviceRequest;
    }

    public async Task<ServiceRequest?> Handle(AddCustomerFeedbackToServiceRequestCommand command)
    {
        var serviceRequest = await serviceRequestRepository.FindByIdAsync(command.ServiceRequestId);
        if (serviceRequest == null) return null;

        serviceRequest.AddCustomerFeedback(command.Rating);
        serviceRequestRepository.Update(serviceRequest);
        await unitOfWork.CompleteAsync();

        // NOTE: Feedback update for WorkOrder is now handled by WorkOrder BC directly
        // No cross-BC coupling needed here

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

    /// <summary>
    /// Provider accepts a service request from the marketplace (Uber-style)
    /// </summary>
    public async Task<ServiceRequest?> Handle(AcceptServiceRequestCommand command)
    {
        var serviceRequest = await serviceRequestRepository.FindByIdAsync(command.ServiceRequestId);
        if (serviceRequest == null)
            return null;

        try
        {
            serviceRequest.AcceptByProvider(command.ProviderId);
            serviceRequestRepository.Update(serviceRequest);
            await unitOfWork.CompleteAsync();

            // Generate notification for owner
            try
            {
                var providerCompanyName = await profilesFacade.FetchProviderCompanyName(command.ProviderId);
                var equipmentExists = await equipmentFacade.EquipmentExists(serviceRequest.EquipmentId);

                if (!string.IsNullOrEmpty(providerCompanyName) && equipmentExists)
                {
                    // Create in-app notification
                    var message = $"{providerCompanyName} has accepted your service request: {serviceRequest.Title}";
                    await notificationFacade.CreateInAppNotification(
                        serviceRequest.ClientId,
                        "✅ Service Request Accepted",
                        message
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ServiceRequest] Error generating notification: {ex.Message}");
                // Don't fail the request if notification fails
            }

            return serviceRequest;
        }
        catch (InvalidOperationException)
        {
            // Service request already accepted by another provider
            return null;
        }
    }
}
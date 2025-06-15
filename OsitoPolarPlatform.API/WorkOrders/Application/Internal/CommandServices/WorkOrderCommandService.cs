using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.WorkOrders.Domain.Repositories;
using OsitoPolarPlatform.API.WorkOrders.Domain.Services;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Commands; 
using OsitoPolarPlatform.API.Shared.Domain.Repositories; 
using OsitoPolarPlatform.API.ServiceRequests.Domain.Repositories; 
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.ServiceRequests.Application.Internal.CommandServices;

/// <summary>
/// Concrete implementation of IWorkOrderCommandService. Handles all command operations for Work Orders.
/// </summary>
public class WorkOrderCommandService(
    IWorkOrderRepository workOrderRepository,
    IServiceRequestRepository serviceRequestRepository,
    IUnitOfWork unitOfWork) : IWorkOrderCommandService
{
    public async Task<WorkOrder?> Handle(CreateWorkOrderCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            throw new ArgumentException("Title is required.");
        if (string.IsNullOrWhiteSpace(command.Description))
            throw new ArgumentException("Description is required.");
        if (string.IsNullOrWhiteSpace(command.IssueDetails))
            throw new ArgumentException("Issue Details are required.");
        if (command.EquipmentId <= 0)
            throw new ArgumentException("Equipment ID must be a positive integer.");
        if (command.ServiceType == default) 
            throw new ArgumentException("Service Type is required.");
        if (string.IsNullOrWhiteSpace(command.ServiceAddress))
            throw new ArgumentException("Service Address is required.");
        if (command.Priority == default) 
            throw new ArgumentException("Priority is required.");

        WorkOrder workOrder;

        if (command.ServiceRequestId.HasValue)
        {
            var serviceRequest = await serviceRequestRepository.FindByIdAsync(command.ServiceRequestId.Value);
            if (serviceRequest == null)
            {
                throw new ArgumentException($"ServiceRequest with ID {command.ServiceRequestId.Value} not found.");
            }

            var existingWorkOrder = await workOrderRepository.FindByServiceRequestIdAsync(serviceRequest.Id);
            if (existingWorkOrder != null)
            {
                throw new InvalidOperationException($"A WorkOrder already exists for ServiceRequest ID {serviceRequest.Id}.");
            }

            serviceRequest.UpdateStatus(EServiceRequestStatus.Accepted);

            workOrder = new WorkOrder(
                serviceRequest.Id, 
                command.Title,
                command.Description,
                command.IssueDetails,
                command.EquipmentId,
                command.ServiceType,
                command.Priority,
                command.Urgency ?? EUrgency.Normal, 
                command.IsEmergency ?? false,      
                command.ScheduledDate,
                command.TimeSlot,
                command.ServiceAddress
            );
        }
        else
        {
            workOrder = new WorkOrder(
                command.Title,
                command.Description,
                command.IssueDetails,
                command.EquipmentId,
                command.ServiceType,
                command.ServiceAddress, 
                command.Priority,
                command.ScheduledDate,
                command.TimeSlot
            );
        }

        await workOrderRepository.AddAsync(workOrder);
        await unitOfWork.CompleteAsync(); 

        return workOrder;
    }

    public async Task<WorkOrder?> Handle(UpdateWorkOrderStatusCommand command)
    {
        var workOrder = await workOrderRepository.FindByIdAsync(command.WorkOrderId);
        if (workOrder is null) return null;

        workOrder.UpdateStatus(command.NewStatus);
        // workOrderRepository.Update(workOrder); 
        await unitOfWork.CompleteAsync();
        return workOrder;
    }

    public async Task<WorkOrder?> Handle(AssignTechnicianToWorkOrderCommand command)
    {
        var workOrder = await workOrderRepository.FindByIdAsync(command.WorkOrderId);
        if (workOrder is null) return null;
        
        if (command.TechnicianId <= 0)
            throw new ArgumentException("Technician ID must be a positive integer.");

        workOrder.AssignTechnician(command.TechnicianId);
        // workOrderRepository.Update(workOrder);
        await unitOfWork.CompleteAsync();
        return workOrder;
    }

    public async Task<WorkOrder?> Handle(AddWorkOrderResolutionDetailsCommand command)
    {
        var workOrder = await workOrderRepository.FindByIdAsync(command.WorkOrderId);
        if (workOrder is null) return null;
        
        if (string.IsNullOrWhiteSpace(command.ResolutionDetails))
            throw new ArgumentException("Resolution Details are required.");

        workOrder.AddResolutionDetails(command.ResolutionDetails, command.TechnicianNotes, command.Cost);
        // workOrderRepository.Update(workOrder);
        await unitOfWork.CompleteAsync();
        return workOrder;
    }

    public async Task<WorkOrder?> Handle(AddWorkOrderCustomerFeedbackCommand command)
    {
        var workOrder = await workOrderRepository.FindByIdAsync(command.WorkOrderId);
        if (workOrder is null) return null;
        
        if (command.Rating < 1 || command.Rating > 5)
            throw new ArgumentOutOfRangeException(nameof(command.Rating), "Rating must be between 1 and 5.");

        workOrder.AddCustomerFeedback(command.Rating, command.Comment);
        // workOrderRepository.Update(workOrder);
        await unitOfWork.CompleteAsync();

        // if (workOrder.AssignedTechnicianId.HasValue) { /* publish event */ }

        return workOrder;
    }
}
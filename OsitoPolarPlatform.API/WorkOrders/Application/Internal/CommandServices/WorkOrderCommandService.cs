using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.WorkOrders.Domain.Repositories;
using OsitoPolarPlatform.API.WorkOrders.Domain.Services; 
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Commands;
using OsitoPolarPlatform.API.Shared.Domain.Repositories; 
using OsitoPolarPlatform.API.ServiceRequests.Domain.Repositories; 
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects; 

namespace OsitoPolarPlatform.API.WorkOrders.Application.Internal.CommandServices;

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

        workOrder.AssignTechnician(command.TechnicianId);
        // workOrderRepository.Update(workOrder);
        await unitOfWork.CompleteAsync();
        return workOrder;
    }

    public async Task<WorkOrder?> Handle(AddWorkOrderResolutionDetailsCommand command)
    {
        var workOrder = await workOrderRepository.FindByIdAsync(command.WorkOrderId);
        if (workOrder is null) return null;

        workOrder.AddResolutionDetails(command.ResolutionDetails, command.TechnicianNotes, command.Cost);
        // workOrderRepository.Update(workOrder);
        await unitOfWork.CompleteAsync();
        return workOrder;
    }

    public async Task<WorkOrder?> Handle(AddWorkOrderCustomerFeedbackCommand command)
    {
        var workOrder = await workOrderRepository.FindByIdAsync(command.WorkOrderId);
        if (workOrder is null) return null;

        workOrder.AddCustomerFeedback(command.Rating, command.Comment);
        // workOrderRepository.Update(workOrder);
        await unitOfWork.CompleteAsync();

        // TODO: Publish a domain event for technician rating if using MediatR/event bus.
        // if (workOrder.AssignedTechnicianId.HasValue) { /* publish event */ }

        return workOrder;
    }
}
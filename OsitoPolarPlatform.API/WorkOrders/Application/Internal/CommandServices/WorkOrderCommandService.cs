using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.WorkOrders.Domain.Repositories;
using OsitoPolarPlatform.API.WorkOrders.Domain.Services;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Domain.Services;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Commands;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Events;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.WorkOrders.Application.Internal.CommandServices;

/// <summary>
/// Concrete implementation of IWorkOrderCommandService. Handles all command operations for Work Orders.
/// </summary>
public class WorkOrderCommandService(
    IWorkOrderRepository workOrderRepository,
    IUnitOfWork unitOfWork,
    IEventBus eventBus) : IWorkOrderCommandService
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
            // Check if WorkOrder already exists for this ServiceRequest
            var existingWorkOrder = await workOrderRepository.FindByServiceRequestIdAsync(command.ServiceRequestId.Value);
            if (existingWorkOrder != null)
            {
                throw new InvalidOperationException($"A WorkOrder already exists for ServiceRequest ID {command.ServiceRequestId.Value}.");
            }

            workOrder = new WorkOrder(
                command.ServiceRequestId.Value,
                command.Title,
                command.Description,
                command.IssueDetails,
                command.EquipmentId,
                command.ServiceType,
                command.Priority,
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
        if (workOrder == null) return null;

        workOrder.UpdateStatus(command.NewStatus);
        workOrderRepository.Update(workOrder);
        await unitOfWork.CompleteAsync();

        // Publish event for status changes that should update ServiceRequest
        if (workOrder.ServiceRequestId.HasValue)
        {
            var statusEvent = new WorkOrderStatusChangedEvent(
                workOrder.Id,
                workOrder.ServiceRequestId.Value,
                command.NewStatus.ToString(),
                null
            );

            await eventBus.PublishAsync(statusEvent);
            Console.WriteLine($"[WorkOrders BC] Published WorkOrderStatusChangedEvent for WO {workOrder.Id}, Status: {command.NewStatus}");
        }

        return workOrder;
    }

    public async Task<WorkOrder?> Handle(AssignTechnicianToWorkOrderCommand command)
    {
        var workOrder = await workOrderRepository.FindByIdAsync(command.WorkOrderId);
        if (workOrder == null) return null;

        workOrder.AssignTechnician(command.TechnicianId);
        workOrderRepository.Update(workOrder); 
        await unitOfWork.CompleteAsync();
        return workOrder;
    }

    public async Task<WorkOrder?> Handle(AddWorkOrderResolutionDetailsCommand command)
    {
        var workOrder = await workOrderRepository.FindByIdAsync(command.WorkOrderId);
        if (workOrder == null) return null;

        workOrder.AddResolutionDetails(command.ResolutionDetails, command.TechnicianNotes, command.Cost);
        workOrderRepository.Update(workOrder);
        await unitOfWork.CompleteAsync();

        // Publish event if WorkOrder is resolved
        if (workOrder.ServiceRequestId.HasValue && workOrder.Status == EWorkOrderStatus.Resolved)
        {
            var statusEvent = new WorkOrderStatusChangedEvent(
                workOrder.Id,
                workOrder.ServiceRequestId.Value,
                "Resolved",
                command.ResolutionDetails
            );

            await eventBus.PublishAsync(statusEvent);
            Console.WriteLine($"[WorkOrders BC] Published WorkOrderStatusChangedEvent for WO {workOrder.Id}, Status: Resolved");
        }

        return workOrder;
    }
}
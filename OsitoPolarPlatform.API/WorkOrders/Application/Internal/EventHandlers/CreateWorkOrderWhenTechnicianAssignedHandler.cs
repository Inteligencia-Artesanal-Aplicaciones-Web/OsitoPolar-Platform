using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Events;
using OsitoPolarPlatform.API.Shared.Application.Internal.EventHandlers;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.WorkOrders.Domain.Repositories;

namespace OsitoPolarPlatform.API.WorkOrders.Application.Internal.EventHandlers;

/// <summary>
/// Event handler that creates a WorkOrder when a Technician is assigned to a ServiceRequest
/// This maintains separation between ServiceRequests and WorkOrders bounded contexts
/// </summary>
public class CreateWorkOrderWhenTechnicianAssignedHandler : IEventHandler<TechnicianAssignedToServiceRequestEvent>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWorkOrderWhenTechnicianAssignedHandler(
        IWorkOrderRepository workOrderRepository,
        IUnitOfWork unitOfWork)
    {
        _workOrderRepository = workOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TechnicianAssignedToServiceRequestEvent notification, CancellationToken cancellationToken)
    {
        // Create WorkOrder from ServiceRequest data (using Value Objects directly)
        var workOrder = new WorkOrder(
            notification.ServiceRequestId,
            notification.Title,
            notification.Description,
            notification.IssueDetails,
            notification.EquipmentId,
            notification.ServiceType,  // EServiceType
            notification.Priority,      // EPriority
            notification.ScheduledDate, // DateTimeOffset?
            notification.TimeSlot ?? string.Empty,
            notification.ServiceAddress ?? string.Empty
        );

        // Assign the technician to the work order
        workOrder.AssignTechnician(notification.TechnicianId);

        // Persist the work order
        await _workOrderRepository.AddAsync(workOrder);
        await _unitOfWork.CompleteAsync();

        Console.WriteLine($"[WorkOrders BC] WorkOrder created for ServiceRequest {notification.ServiceRequestId} with Technician {notification.TechnicianId}");
    }
}

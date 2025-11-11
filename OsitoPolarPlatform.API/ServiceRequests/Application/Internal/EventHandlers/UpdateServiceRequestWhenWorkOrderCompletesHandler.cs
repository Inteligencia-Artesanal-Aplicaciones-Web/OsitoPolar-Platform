using OsitoPolarPlatform.API.ServiceRequests.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Application.Internal.EventHandlers;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Events;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.ServiceRequests.Application.Internal.EventHandlers;

/// <summary>
/// Event handler that updates ServiceRequest status when WorkOrder is completed
/// Maintains separation between WorkOrders and ServiceRequests bounded contexts
/// </summary>
public class UpdateServiceRequestWhenWorkOrderCompletesHandler : IEventHandler<WorkOrderStatusChangedEvent>
{
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceRequestWhenWorkOrderCompletesHandler(
        IServiceRequestRepository serviceRequestRepository,
        IUnitOfWork unitOfWork)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(WorkOrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        var serviceRequest = await _serviceRequestRepository.FindByIdAsync(notification.ServiceRequestId);
        if (serviceRequest == null)
        {
            Console.WriteLine($"[ServiceRequests BC] ServiceRequest {notification.ServiceRequestId} not found for WorkOrder {notification.WorkOrderId}");
            return;
        }

        // Update ServiceRequest status based on WorkOrder status
        if (notification.NewStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
            notification.NewStatus.Equals("Resolved", StringComparison.OrdinalIgnoreCase))
        {
            serviceRequest.UpdateStatus(EServiceRequestStatus.Resolved);
            _serviceRequestRepository.Update(serviceRequest);
            await _unitOfWork.CompleteAsync();

            Console.WriteLine($"[ServiceRequests BC] ServiceRequest {notification.ServiceRequestId} marked as Resolved due to WorkOrder {notification.WorkOrderId} completion");
        }
    }
}

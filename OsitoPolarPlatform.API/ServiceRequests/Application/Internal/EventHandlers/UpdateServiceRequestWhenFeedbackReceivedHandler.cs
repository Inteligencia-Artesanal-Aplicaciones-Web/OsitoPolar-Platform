using OsitoPolarPlatform.API.ServiceRequests.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Application.Internal.EventHandlers;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Events;

namespace OsitoPolarPlatform.API.ServiceRequests.Application.Internal.EventHandlers;

/// <summary>
/// Event handler that updates ServiceRequest when WorkOrder receives feedback
/// Maintains separation between WorkOrders and ServiceRequests bounded contexts
/// </summary>
public class UpdateServiceRequestWhenFeedbackReceivedHandler : IEventHandler<WorkOrderFeedbackReceivedEvent>
{
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceRequestWhenFeedbackReceivedHandler(
        IServiceRequestRepository serviceRequestRepository,
        IUnitOfWork unitOfWork)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(WorkOrderFeedbackReceivedEvent notification, CancellationToken cancellationToken)
    {
        var serviceRequest = await _serviceRequestRepository.FindByIdAsync(notification.ServiceRequestId);
        if (serviceRequest == null)
        {
            Console.WriteLine($"[ServiceRequests BC] ServiceRequest {notification.ServiceRequestId} not found for feedback from WorkOrder {notification.WorkOrderId}");
            return;
        }

        // Update ServiceRequest with feedback rating
        serviceRequest.AddCustomerFeedback(notification.Rating);
        _serviceRequestRepository.Update(serviceRequest);
        await _unitOfWork.CompleteAsync();

        Console.WriteLine($"[ServiceRequests BC] ServiceRequest {notification.ServiceRequestId} updated with feedback rating {notification.Rating}");
    }
}

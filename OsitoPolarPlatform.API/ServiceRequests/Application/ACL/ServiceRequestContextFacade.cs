using OsitoPolarPlatform.API.ServiceRequests.Domain.Repositories;
using OsitoPolarPlatform.API.ServiceRequests.Interfaces.ACL;

namespace OsitoPolarPlatform.API.ServiceRequests.Application.ACL;

/// <summary>
/// Facade implementation for the Service Requests context
/// </summary>
/// <param name="serviceRequestRepository">The service request repository</param>
public class ServiceRequestContextFacade(IServiceRequestRepository serviceRequestRepository) : IServiceRequestContextFacade
{
    public async Task<bool> ServiceRequestExists(int serviceRequestId)
    {
        var serviceRequest = await serviceRequestRepository.FindByIdAsync(serviceRequestId);
        return serviceRequest != null;
    }

    public async Task<string> FetchServiceRequestStatus(int serviceRequestId)
    {
        var serviceRequest = await serviceRequestRepository.FindByIdAsync(serviceRequestId);
        return serviceRequest?.Status.ToString() ?? string.Empty;
    }

    public async Task<int> FetchServiceRequestEquipmentId(int serviceRequestId)
    {
        var serviceRequest = await serviceRequestRepository.FindByIdAsync(serviceRequestId);
        return serviceRequest?.EquipmentId ?? 0;
    }

    public async Task<string> FetchServiceRequestTitle(int serviceRequestId)
    {
        var serviceRequest = await serviceRequestRepository.FindByIdAsync(serviceRequestId);
        return serviceRequest?.Title ?? string.Empty;
    }

    public async Task<int> CountActiveServiceRequestsByEquipmentId(int equipmentId)
    {
        var requests = await serviceRequestRepository.FindByEquipmentIdAsync(equipmentId);
        return requests.Count(sr =>
            sr.Status.ToString() == "Pending" ||
            sr.Status.ToString() == "InProgress");
    }

    public async Task<int> CountAllActiveServiceRequests()
    {
        var allRequests = await serviceRequestRepository.ListAsync();
        return allRequests.Count(sr =>
            sr.Status.ToString() == "Pending" ||
            sr.Status.ToString() == "InProgress");
    }

    public async Task<(int id, int clientId, int companyId)?> GetServiceRequestData(int serviceRequestId)
    {
        var serviceRequest = await serviceRequestRepository.FindByIdAsync(serviceRequestId);
        if (serviceRequest == null) return null;

        return (serviceRequest.Id, serviceRequest.ClientId, serviceRequest.CompanyId);
    }
}

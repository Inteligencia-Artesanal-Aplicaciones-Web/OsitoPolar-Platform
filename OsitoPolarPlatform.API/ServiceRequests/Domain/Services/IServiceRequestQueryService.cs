using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Queries;

namespace OsitoPolarPlatform.API.ServiceRequests.Domain.Services;

public interface IServiceRequestQueryService
{
    Task<ServiceRequest?> Handle(GetServiceRequestByIdQuery query);
    Task<IEnumerable<ServiceRequest>> Handle(GetAllServiceRequestsQuery query);
    Task<IEnumerable<ServiceRequest>> Handle(GetServiceRequestsByStatusQuery query);
    Task<IEnumerable<ServiceRequest>> Handle(GetServiceRequestsByEquipmentIdQuery query);
}

using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Queries;

public record GetServiceRequestsByStatusQuery(EServiceRequestStatus Status);

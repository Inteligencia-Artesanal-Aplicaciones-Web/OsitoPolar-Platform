using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Commands;

public record UpdateServiceRequestStatusCommand(int ServiceRequestId, EServiceRequestStatus NewStatus);

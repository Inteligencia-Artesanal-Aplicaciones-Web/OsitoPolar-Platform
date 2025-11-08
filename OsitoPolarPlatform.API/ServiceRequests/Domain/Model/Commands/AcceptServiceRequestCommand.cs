namespace OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Commands;

/// <summary>
/// Command to accept a service request by a provider (Uber-style marketplace)
/// </summary>
public record AcceptServiceRequestCommand(int ServiceRequestId, int ProviderId);

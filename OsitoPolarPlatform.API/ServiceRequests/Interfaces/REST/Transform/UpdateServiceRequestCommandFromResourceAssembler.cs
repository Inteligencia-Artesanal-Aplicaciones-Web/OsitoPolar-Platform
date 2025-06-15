using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Commands;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects;
using OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Transform;


public static class UpdateServiceRequestCommandFromResourceAssembler
{
    public static UpdateServiceRequestCommand ToCommandFromResource(int id, UpdateServiceRequestResource resource)
    {
        Enum.TryParse(resource.Status, true, out EServiceRequestStatus status);
        Enum.TryParse(resource.Priority, true, out EPriority priority);
        Enum.TryParse(resource.Urgency, true, out EUrgency urgency);
        Enum.TryParse(resource.ServiceType, true, out EServiceType serviceType);

        return new UpdateServiceRequestCommand(
            id,
            resource.Title,
            resource.Description,
            resource.IssueDetails,
            status,
            priority,
            urgency,
            resource.IsEmergency,
            serviceType, 
            resource.AssignedTechnicianId,
            resource.ScheduledDate,
            resource.TimeSlot,
            resource.ServiceAddress
        );
    }
}

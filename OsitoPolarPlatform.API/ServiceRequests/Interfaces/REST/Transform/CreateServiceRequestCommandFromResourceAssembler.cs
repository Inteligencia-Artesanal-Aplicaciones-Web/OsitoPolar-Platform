using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Commands;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects;
using OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Transform;

public static class CreateServiceRequestCommandFromResourceAssembler
{
    public static CreateServiceRequestCommand ToCommandFromResource(CreateServiceRequestResource resource)
    {
        Enum.TryParse(resource.Priority, true, out EPriority priority);
        Enum.TryParse(resource.Urgency, true, out EUrgency urgency);
        Enum.TryParse(resource.ServiceType, true, out EServiceType serviceType); 
        return new CreateServiceRequestCommand(
            resource.Title,
            resource.Description,
            resource.IssueDetails,
            resource.EquipmentId,
            serviceType, 
            resource.ReportedByUserId,
            priority,
            urgency,
            resource.IsEmergency,
            resource.ScheduledDate,
            resource.TimeSlot,
            resource.ServiceAddress
        );
    }
}

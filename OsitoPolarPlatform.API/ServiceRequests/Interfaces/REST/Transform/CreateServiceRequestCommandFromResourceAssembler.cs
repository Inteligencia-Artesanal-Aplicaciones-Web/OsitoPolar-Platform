using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Commands;
using OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Transform;

public static class CreateServiceRequestCommandFromResourceAssembler
{
    public static CreateServiceRequestCommand ToCommandFromResource(CreateServiceRequestResource resource)
    {
        return new CreateServiceRequestCommand(
            resource.Title,
            resource.Description,
            resource.IssueDetails,
            resource.ClientId, 
            resource.CompanyId,
            resource.EquipmentId,
            resource.ServiceType,
            resource.ReportedByUserId,
            resource.Priority,
            resource.Urgency,
            resource.IsEmergency,
            resource.ScheduledDate,
            resource.TimeSlot,
            resource.ServiceAddress
        );
    }
}
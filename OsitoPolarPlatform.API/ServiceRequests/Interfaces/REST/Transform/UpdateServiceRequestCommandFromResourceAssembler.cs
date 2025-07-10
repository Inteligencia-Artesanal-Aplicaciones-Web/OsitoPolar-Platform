using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Commands;
using OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Transform;

public static class UpdateServiceRequestCommandFromResourceAssembler
{
    public static UpdateServiceRequestCommand ToCommandFromResource(int id, UpdateServiceRequestResource resource)
    {
        return new UpdateServiceRequestCommand(
            id,
            resource.Title,
            resource.Description,
            resource.IssueDetails,
            resource.Status,       
            resource.Priority,     
            resource.Urgency,     
            resource.IsEmergency,
            resource.ServiceType,  
            resource.AssignedTechnicianId,
            resource.ScheduledDate,
            resource.TimeSlot,
            resource.ServiceAddress
        );
    }
}
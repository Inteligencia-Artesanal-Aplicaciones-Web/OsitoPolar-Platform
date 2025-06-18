using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Commands;
using OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Resources;
namespace OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Transform;

/// <summary>
/// Assembles a CreateWorkOrderCommand from a CreateWorkOrderResource.
/// </summary>
public static class CreateWorkOrderCommandFromResourceAssembler
{
    public static CreateWorkOrderCommand ToCommandFromResource(CreateWorkOrderResource resource)
    {
        return new CreateWorkOrderCommand(
            resource.Title,
            resource.Description,
            resource.IssueDetails,
            resource.EquipmentId,
            resource.ServiceType,
            resource.ServiceAddress,
            resource.Priority,
            resource.ServiceRequestId
            //ReportedByUserId: null, 
            //Urgency: null, 
            //resource.ScheduledDate,
            //resource.TimeSlot ?? string.Empty
        );
    }
}
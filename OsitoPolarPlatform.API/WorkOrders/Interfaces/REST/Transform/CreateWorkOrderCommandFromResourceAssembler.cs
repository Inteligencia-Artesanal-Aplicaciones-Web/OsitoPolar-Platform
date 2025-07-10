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
            Title: resource.Title,
            Description: resource.Description,
            IssueDetails: resource.IssueDetails,
            EquipmentId: resource.EquipmentId,
            ServiceType: resource.ServiceType,
            ServiceAddress: resource.ServiceAddress,
            Priority: resource.Priority,
            ServiceRequestId: resource.ServiceRequestId,
            ReportedByUserId: null,  
            Urgency: null,           
            IsEmergency: null,       
            ScheduledDate: resource.ScheduledDate,
            TimeSlot: resource.TimeSlot ?? string.Empty
        );
    }
}
using OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Aggregates;
using Microsoft.OpenApi.Extensions;

namespace OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Transform;

public static class ServiceRequestResourceFromEntityAssembler
{
    public static ServiceRequestResource ToResourceFromEntity(ServiceRequest entity)
    {
        return new ServiceRequestResource(
            entity.Id,
            entity.OrderNumber,
            entity.Title,
            entity.Description,
            entity.IssueDetails,
            entity.ClientId,
            entity.CompanyId,
            entity.EquipmentId,
            entity.RequestTime,
            entity.Status.ToString(), 
            entity.Priority.ToString(),
            entity.Urgency.ToString(),
            entity.IsEmergency,
            entity.ServiceType.ToString(),
            //entity.ReportedByUser != null ? UserResourceFromEntityAssembler.ToResourceFromEntity(entity.ReportedByUser) : null,
            //EquipmentResourceFromEntityAssembler.ToResourceFromEntity(entity.Equipment),
            //entity.AssignedTechnician != null ? TechnicianResourceFromEntityAssembler.ToResourceFromEntity(entity.AssignedTechnician) : null,
            entity.ScheduledDate,
            entity.TimeSlot,
            entity.ServiceAddress,
            entity.DesiredCompletionDate,
            entity.ActualCompletionDate,
            //entity.ResolutionDetails,
            //entity.TechnicianNotes,
            //entity.Cost,
            entity.CustomerFeedbackRating
        );
    }
}
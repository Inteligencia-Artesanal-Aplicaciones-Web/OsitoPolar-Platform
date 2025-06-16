using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Transform;

/// <summary>
/// Assembles a WorkOrderResource from a WorkOrder entity.
/// </summary>
public static class WorkOrderResourceFromEntityAssembler
{
    public static WorkOrderResource ToResourceFromEntity(WorkOrder entity)
    {
        return new WorkOrderResource(
            entity.Id,
            entity.WorkOrderNumber,
            entity.ServiceRequestId,
            entity.Title,
            entity.Description,
            entity.IssueDetails,
            entity.CreationTime,
            entity.Status.ToString(), 
            entity.Priority.ToString(), 
            entity.AssignedTechnicianId,
            entity.ScheduledDate,
            entity.TimeSlot,
            entity.ServiceAddress,
            entity.DesiredCompletionDate,
            entity.ActualCompletionDate,
            entity.ResolutionDetails,
            entity.TechnicianNotes,
            entity.Cost,
            entity.CustomerFeedbackRating,
            entity.FeedbackSubmissionDate,
            entity.EquipmentId, 
            entity.ServiceType.ToString() 
        );
    }
}
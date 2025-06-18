namespace OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Resources;

/// <summary>
/// Resource for representing a WorkOrder in API responses.
/// </summary>
public record WorkOrderResource(
    int Id,
    string WorkOrderNumber,
    int? ServiceRequestId,
    string Title,
    string Description,
    string IssueDetails,
    DateTimeOffset CreationTime,
    string Status, 
    string Priority, 
    int? AssignedTechnicianId,
    DateTimeOffset? ScheduledDate,
    string TimeSlot,
    string ServiceAddress,
    DateTimeOffset? DesiredCompletionDate,
    DateTimeOffset? ActualCompletionDate,
    string ResolutionDetails,
    string TechnicianNotes,
    decimal? Cost,
    int? CustomerFeedbackRating,
    DateTimeOffset? FeedbackSubmissionDate,
    int EquipmentId,
    string ServiceType 
);
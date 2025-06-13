namespace OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Resources;

public record UpdateServiceRequestResource(
    string Title,
    string Description,
    string IssueDetails,
    string Status,
    string Priority,
    string Urgency,
    bool IsEmergency,
    string ServiceType, 
    int? AssignedTechnicianId,
    DateTimeOffset? ScheduledDate,
    string TimeSlot,
    string ServiceAddress
);

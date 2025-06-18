namespace OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Resources;

public record CreateServiceRequestResource(
    string Title,
    string Description,
    string IssueDetails,
    int ClientId, 
    int CompanyId,
    int EquipmentId,
    string ServiceType,
    int? ReportedByUserId,
    string Priority,
    string Urgency,
    bool IsEmergency,
    DateTimeOffset? ScheduledDate,
    string TimeSlot,
    string ServiceAddress
);
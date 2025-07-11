using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Resources;

public record CreateServiceRequestResource(
    string Title,
    string Description,
    string IssueDetails,
    int ClientId, 
    int CompanyId,
    int EquipmentId,
    EServiceType ServiceType,  
    int? ReportedByUserId,
    EPriority Priority,        
    EUrgency Urgency,         
    bool IsEmergency,
    DateTimeOffset? ScheduledDate,
    string TimeSlot,
    string ServiceAddress
);
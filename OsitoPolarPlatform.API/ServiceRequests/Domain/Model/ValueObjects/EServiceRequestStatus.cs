namespace OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects;

public enum EServiceRequestStatus 
{
    Pending = 1,
    Accepted = 2,
    InProgress = 3,
    Resolved = 4,
    Cancelled = 5,
    Rejected = 6
}

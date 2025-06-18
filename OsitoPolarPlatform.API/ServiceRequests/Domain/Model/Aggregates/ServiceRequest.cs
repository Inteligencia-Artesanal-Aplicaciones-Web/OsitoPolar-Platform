using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Aggregates;

/// <summary>
/// Represents a service request for a refrigeration equipment.
/// </summary>
public partial class ServiceRequest
{
    public int Id { get; private set; }
    public string OrderNumber { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string IssueDetails { get; private set; }
    public DateTimeOffset RequestTime { get; private set; }
    public EServiceRequestStatus Status { get; private set; }
    public EPriority Priority { get; private set; }
    public EUrgency Urgency { get; private set; }
    public bool IsEmergency { get; private set; }
    public EServiceType ServiceType { get; private set; }

    public int ClientId { get; private set; } 
    public int CompanyId { get; private set; }
    public int EquipmentId { get; private set; }
    public int? AssignedTechnicianId { get; private set; }
    public DateTimeOffset? ScheduledDate { get; private set; }
    public string TimeSlot { get; private set; }
    public string ServiceAddress { get; private set; }

    public DateTimeOffset? DesiredCompletionDate { get; private set; }
    public DateTimeOffset? ActualCompletionDate { get; private set; }
    public string ResolutionDetails { get; private set; }
    public string TechnicianNotes { get; private set; }
    public decimal? Cost { get; private set; }
    public int? CustomerFeedbackRating { get; private set; }
    public DateTimeOffset? FeedbackSubmissionDate { get; private set; }
    
    protected ServiceRequest()
    {
        OrderNumber = Guid.NewGuid().ToString();
        Title = string.Empty;
        Description = string.Empty;
        IssueDetails = string.Empty;
        RequestTime = DateTimeOffset.UtcNow;
        Status = EServiceRequestStatus.Pending;
        Priority = EPriority.Medium;
        Urgency = EUrgency.Normal;
        IsEmergency = false;
        ServiceType = EServiceType.Diagnostic;
        TimeSlot = string.Empty;
        ServiceAddress = string.Empty;
        ResolutionDetails = string.Empty;
        TechnicianNotes = string.Empty;
    }

    public ServiceRequest(
        string title,
        string description,
        string issueDetails,
        int clientId,
        int companyId,
        int equipmentId,
        EServiceType serviceType,
        EPriority priority = EPriority.Medium,
        EUrgency urgency = EUrgency.Normal,
        bool isEmergency = false,
        DateTimeOffset? scheduledDate = null,
        string timeSlot = "",
        string serviceAddress = "") : this()
    {
        Title = title;
        Description = description;
        IssueDetails = issueDetails;
        ClientId = clientId;
        CompanyId = companyId;
        EquipmentId = equipmentId;
        ServiceType = serviceType;
        Priority = priority;
        Urgency = urgency;
        IsEmergency = isEmergency;
        ScheduledDate = scheduledDate;
        TimeSlot = timeSlot;
        ServiceAddress = serviceAddress;
    }

    public void AssignTechnician(int technicianId)
    {
        if (technicianId <= 0)
            throw new ArgumentException("Technician ID must be positive.", nameof(technicianId));
        if (Status != EServiceRequestStatus.Pending)
            throw new InvalidOperationException("Only pending service requests can be assigned a technician.");

        AssignedTechnicianId = technicianId;
        Status = EServiceRequestStatus.Accepted; 
    }

    public void UpdateStatus(EServiceRequestStatus newStatus)
    {
        Status = newStatus;
        if (newStatus == EServiceRequestStatus.Resolved)
        {
            ActualCompletionDate = DateTimeOffset.UtcNow;
        }
    }
    
    public void Reject()
    {
        if (Status == EServiceRequestStatus.Accepted || Status == EServiceRequestStatus.InProgress || Status == EServiceRequestStatus.Resolved || Status == EServiceRequestStatus.Cancelled || Status == EServiceRequestStatus.Rejected)
            throw new InvalidOperationException("Service request cannot be rejected from its current status.");
        Status = EServiceRequestStatus.Rejected;
    }
    
    public void Cancel()
    {
        if (Status == EServiceRequestStatus.Resolved || Status == EServiceRequestStatus.Cancelled || Status == EServiceRequestStatus.Rejected)
            throw new InvalidOperationException("Service request cannot be cancelled from its current status.");
        Status = EServiceRequestStatus.Cancelled;
    }

    public void AddResolutionDetails(string resolution, string technicianNotes, decimal? cost)
    {
        ResolutionDetails = resolution;
        TechnicianNotes = technicianNotes;
        Cost = cost;
        Status = EServiceRequestStatus.Resolved;
    }
    
    public void AddCustomerFeedback(int rating)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5.");
        if (Status != EServiceRequestStatus.Resolved) 
            throw new InvalidOperationException("Cannot add feedback to an unresolved service request.");

        CustomerFeedbackRating = rating;
        FeedbackSubmissionDate = DateTimeOffset.UtcNow; 
    }
}
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.ValueObjects;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.WorkOrders.Domain.Model.Aggregates;

/// <summary>
/// Represents a work order for refrigeration equipment service,
/// managing its lifecycle from creation to completion and customer feedback.
/// </summary>
public partial class WorkOrder
{
    public int Id { get; private set; }
    public string WorkOrderNumber { get; private set; }
    public int? ServiceRequestId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string IssueDetails { get; private set; }
    public DateTimeOffset CreationTime { get; private set; }
    public EWorkOrderStatus Status { get; private set; }
    public EPriority Priority { get; private set; }
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
    public int EquipmentId { get; private set; }
    public EServiceType ServiceType { get; private set; }

    protected WorkOrder()
    {
        WorkOrderNumber = GenerateWorkOrderNumber();
        Title = string.Empty;
        Description = string.Empty;
        IssueDetails = string.Empty;
        CreationTime = DateTimeOffset.UtcNow;
        Status = EWorkOrderStatus.Created;
        Priority = EPriority.Medium;
        TimeSlot = string.Empty;
        ServiceAddress = string.Empty;
        ResolutionDetails = string.Empty;
        TechnicianNotes = string.Empty;
    }

    private string GenerateWorkOrderNumber()
    {
        return $"WO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
    }

    public WorkOrder(
        int? serviceRequestId,
        string title,
        string description,
        string issueDetails,
        int equipmentId,
        EServiceType serviceType,
        EPriority initialPriority,
        DateTimeOffset? scheduledDate,
        string timeSlot,
        string serviceAddress) : this()
    {
        ServiceRequestId = serviceRequestId;
        Title = title;
        Description = description;
        IssueDetails = issueDetails;
        EquipmentId = equipmentId;
        ServiceType = serviceType;
        Priority = initialPriority;
        ScheduledDate = scheduledDate;
        TimeSlot = timeSlot;
        ServiceAddress = serviceAddress;
    }
    
    public WorkOrder(
        string title,
        string description,
        string issueDetails,
        int equipmentId,
        EServiceType serviceType,
        string serviceAddress,
        EPriority priority = EPriority.Medium,
        DateTimeOffset? scheduledDate = null,
        string timeSlot = "") : this()
    {
        Title = title;
        Description = description;
        IssueDetails = issueDetails;
        EquipmentId = equipmentId;
        ServiceType = serviceType;
        ServiceAddress = serviceAddress;
        Priority = priority;
        ScheduledDate = scheduledDate;
        TimeSlot = timeSlot;
        ServiceRequestId = null; 
    }

    /// <summary>
    /// Assigns a technician to the Work Order and updates its status.
    /// </summary>
    /// <param name="technicianId">The ID of the technician to assign.</param>
    public void AssignTechnician(int technicianId)
    {
        if (technicianId <= 0)
            throw new ArgumentException("Technician ID must be positive.", nameof(technicianId));
        
        AssignedTechnicianId = technicianId; 

        if (Status == EWorkOrderStatus.Created)
        {
            Status = EWorkOrderStatus.Assigned;
        }
    }

    public void UpdateStatus(EWorkOrderStatus newStatus)
    {
        Status = newStatus;

        if (newStatus == EWorkOrderStatus.Completed || newStatus == EWorkOrderStatus.Resolved)
        {
            ActualCompletionDate = DateTimeOffset.UtcNow;
        }
    }

    public void UpdateSchedule(DateTimeOffset? newScheduledDate, string newTimeSlot)
    {
        ScheduledDate = newScheduledDate;
        TimeSlot = newTimeSlot ?? string.Empty;
    }

    public void AddResolutionDetails(string resolution, string technicianNotes, decimal? cost)
    {
        if (string.IsNullOrWhiteSpace(resolution))
            throw new ArgumentException("Resolution details cannot be empty.", nameof(resolution));

        ResolutionDetails = resolution;
        TechnicianNotes = technicianNotes ?? string.Empty;
        Cost = cost;
        Status = EWorkOrderStatus.Resolved;
    }

    public void SetCustomerFeedbackRating(int rating)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5.");
        CustomerFeedbackRating = rating;
        FeedbackSubmissionDate = DateTimeOffset.UtcNow;
    }

    public void UpdatePriority(EPriority newPriority)
    {
        Priority = newPriority;
    }
}
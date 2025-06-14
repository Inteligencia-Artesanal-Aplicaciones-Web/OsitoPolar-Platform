namespace OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Aggregates;

using OsitoPolarPlatform.API.Equipment.Domain.Model.Entities; 
using OsitoPolarPlatform.API.Users.Domain.Model.Entities;     
using OsitoPolarPlatform.API.Technicians.Domain.Model.Entities; 
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects; 
using System.ComponentModel.DataAnnotations.Schema; 

{
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

        
        public int? ReportedByUserId { get; private set; } 
        public User? ReportedByUser { get; private set; } 
        public int EquipmentId { get; private set; } 
        public Equipment.Domain.Model.Entities.Equipment Equipment { get; private set; } 
        public int? AssignedTechnicianId { get; private set; } 
        public Technician? AssignedTechnician { get; private set; } 
        public DateTimeOffset? ScheduledDate { get; private set; } 
        public string TimeSlot { get; private set; } 
        public string ServiceAddress { get; private set; } 
        
        public DateTimeOffset? DesiredCompletionDate { get; private set; }
        public DateTimeOffset? ActualCompletionDate { get; private set; } 
        public string ResolutionDetails { get; private set; } 
        public string TechnicianNotes { get; private set; } 
        public decimal? Cost { get; private set; } 
        public int? CustomerFeedbackRating { get; private set; }

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
            int equipmentId,
            EServiceType serviceType,
            int? reportedByUserId = null,
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
            EquipmentId = equipmentId;
            ServiceType = serviceType; 
            ReportedByUserId = reportedByUserId;
            Priority = priority;
            Urgency = urgency;
            IsEmergency = isEmergency;
            ScheduledDate = scheduledDate;
            TimeSlot = timeSlot;
            ServiceAddress = serviceAddress;
        }

        public void AssignTechnician(int technicianId)
        {
            AssignedTechnicianId = technicianId;
            if (Status == EServiceRequestStatus.Pending)
            {
                Status = EServiceRequestStatus.Accepted;
            }
        }

        public void UpdateStatus(EServiceRequestStatus newStatus)
        {
            Status = newStatus;
            if (newStatus == EServiceRequestStatus.Resolved)
            {
                ActualCompletionDate = DateTimeOffset.UtcNow;
            }
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
            CustomerFeedbackRating = rating;
        }
    }
}
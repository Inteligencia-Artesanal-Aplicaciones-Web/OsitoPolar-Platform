using System.ComponentModel.DataAnnotations;

namespace OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Resources;

public class AddCustomerFeedbackToServiceRequestResource
{
    [Required]
    [Range(1, 5)] 
    public int Rating { get; set; }
}
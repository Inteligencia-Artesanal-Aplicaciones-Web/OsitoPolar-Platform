using System.ComponentModel.DataAnnotations;

namespace OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Resources;

public class AssignTechnicianToServiceRequestResource
{
    [Required]
    public int TechnicianId { get; set; }
}
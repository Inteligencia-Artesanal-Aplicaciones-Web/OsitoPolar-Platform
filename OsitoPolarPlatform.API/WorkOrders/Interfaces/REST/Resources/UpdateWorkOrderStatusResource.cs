using System.ComponentModel.DataAnnotations;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Resources;

public class UpdateWorkOrderStatusResource
{
    [Required]
    public EWorkOrderStatus NewStatus { get; set; } 
}
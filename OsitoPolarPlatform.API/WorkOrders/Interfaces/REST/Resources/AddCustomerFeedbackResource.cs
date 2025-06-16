using System.ComponentModel.DataAnnotations; 

namespace OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Resources;

/// <summary>
/// Resource for adding customer feedback to a WorkOrder.
/// </summary>
public record AddCustomerFeedbackResource(
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")] int Rating
);
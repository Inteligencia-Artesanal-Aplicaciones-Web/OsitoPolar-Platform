using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Commands;
using OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Transform;

/// <summary>
/// Assembles an AddWorkOrderCustomerFeedbackCommand from an AddCustomerFeedbackResource.
/// </summary>
public static class AddWorkOrderCustomerFeedbackCommandFromResourceAssembler
{
    public static AddWorkOrderCustomerFeedbackCommand ToCommandFromResource(int workOrderId, AddCustomerFeedbackResource resource)
    {
        return new AddWorkOrderCustomerFeedbackCommand(
            workOrderId,
            resource.Rating
            );
    }
}
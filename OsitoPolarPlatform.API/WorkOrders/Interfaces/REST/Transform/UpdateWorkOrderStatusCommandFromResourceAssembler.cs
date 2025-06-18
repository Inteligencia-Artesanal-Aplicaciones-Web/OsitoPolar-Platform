using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Commands;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.ValueObjects;
using OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Transform;

public static class UpdateWorkOrderStatusCommandFromResourceAssembler
{
    public static UpdateWorkOrderStatusCommand ToCommandFromResource(int workOrderId, UpdateWorkOrderStatusResource resource)
    {
        return new UpdateWorkOrderStatusCommand(workOrderId, resource.NewStatus);
    }
}
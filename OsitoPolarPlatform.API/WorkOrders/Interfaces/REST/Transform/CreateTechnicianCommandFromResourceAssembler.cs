using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Commands;
using OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Transform;

public static class CreateTechnicianCommandFromResourceAssembler
{
    public static CreateTechnicianCommand ToCommandFromResource(
        CreateTechnicianResource resource)
    {
        return new CreateTechnicianCommand(
            resource.Name,
            resource.Specialization,
            resource.Phone,
            resource.Email,
            resource.Availability,
            resource.CompanyId
        );
    }
}

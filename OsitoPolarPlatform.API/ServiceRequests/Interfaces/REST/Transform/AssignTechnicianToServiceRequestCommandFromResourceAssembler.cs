using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Commands;
using OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Transform;

public static class AssignTechnicianToServiceRequestCommandFromResourceAssembler
{
    public static AssignTechnicianToServiceRequestCommand ToCommandFromResource(int serviceRequestId, AssignTechnicianToServiceRequestResource resource)
    {
        return new AssignTechnicianToServiceRequestCommand(serviceRequestId, resource.TechnicianId);
    }
}
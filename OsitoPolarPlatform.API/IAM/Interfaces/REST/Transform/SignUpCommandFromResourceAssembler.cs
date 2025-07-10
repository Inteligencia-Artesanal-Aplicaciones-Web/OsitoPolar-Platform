using OsitoPolarPlatform.API.IAM.Domain.Model.Commands;
using OsitoPolarPlatform.API.IAM.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.IAM.Interfaces.REST.Transform;

public static class SignUpCommandFromResourceAssembler
{
    public static SignUpCommand ToCommandFromResource(SignUpResource resource)
    {
        return new SignUpCommand(resource.Username, resource.Password);
    }
}
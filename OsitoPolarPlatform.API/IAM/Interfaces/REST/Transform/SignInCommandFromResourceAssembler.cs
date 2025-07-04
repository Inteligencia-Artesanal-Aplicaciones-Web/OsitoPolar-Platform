using OsitoPolarPlatform.API.IAM.Domain.Model.Commands;
using OsitoPolarPlatform.API.IAM.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.IAM.Interfaces.REST.Transform;

public static class SignInCommandFromResourceAssembler
{
    public static SignInCommand ToCommandFromResource(SignInResource resource)
    {
        return new SignInCommand(resource.Username, resource.Password);
    }
}
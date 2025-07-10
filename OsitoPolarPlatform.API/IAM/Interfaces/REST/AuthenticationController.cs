using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OsitoPolarPlatform.API.IAM.Domain.Services;
using OsitoPolarPlatform.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using OsitoPolarPlatform.API.IAM.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.IAM.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace OsitoPolarPlatform.API.IAM.Interfaces.REST;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Authentication endpoints")]
public class AuthenticationController(IUserCommandService userCommandService) : ControllerBase
{
    /**
     * <summary>
     *     Sign in endpoint. It allows authenticating a user
     * </summary>
     * <param name="signInResource">The sign-in resource containing username and password.</param>
     * <returns>The authenticated user resource, including a JWT token</returns>
     */
    [HttpPost("sign-in")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Sign in",
        Description = "Sign in a user",
        OperationId = "SignIn")]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was authenticated", typeof(AuthenticatedUserResource))]
    public async Task<IActionResult> SignIn([FromBody] SignInResource signInResource)
    {
        try
        {
            var signInCommand = SignInCommandFromResourceAssembler.ToCommandFromResource(signInResource);
            var authenticatedUser = await userCommandService.Handle(signInCommand);
            var resource =
                AuthenticatedUserResourceFromEntityAssembler.ToResourceFromEntity(authenticatedUser.user,
                    authenticatedUser.token);
            return Ok(resource);    
        } catch (Exception ex)
        {
            return Unauthorized(new
            {
                message = "An error occurred while sign-in in",
                error = ex.Message
            });
        } 
        
    }

    /**
     * <summary>
     *     Sign up endpoint. It allows creating a new user
     * </summary>
     * <param name="signUpResource">The sign-up resource containing username and password.</param>
     * <returns>A confirmation message on successful creation.</returns>
     */
    [HttpPost("sign-up")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Sign-up",
        Description = "Sign up a new user",
        OperationId = "SignUp")]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was created successfully")]
    public async Task<IActionResult> SignUp([FromBody] SignUpResource signUpResource)
    {
        try
        {
            var signUpCommand = SignUpCommandFromResourceAssembler.ToCommandFromResource(signUpResource);
            await userCommandService.Handle(signUpCommand);
            return Ok(new { message = "User created successfully" });    
        } catch (Exception ex)
        {
            return BadRequest(new
            {
                message = "An error occurred while creating the user.",
                error = ex.Message
            });
        }
        
    }
}
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OsitoPolarPlatform.API.IAM.Application.Internal.OutboundServices;
using OsitoPolarPlatform.API.IAM.Domain.Model.Commands;
using OsitoPolarPlatform.API.IAM.Domain.Repositories;
using OsitoPolarPlatform.API.IAM.Domain.Services;
using OsitoPolarPlatform.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using OsitoPolarPlatform.API.IAM.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.IAM.Interfaces.REST.Transform;
using OsitoPolarPlatform.API.Profiles.Domain.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace OsitoPolarPlatform.API.IAM.Interfaces.REST;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Authentication endpoints")]
public class AuthenticationController(
    IUserCommandService userCommandService,
    IRegistrationService registrationService,
    ITwoFactorService twoFactorService,
    IUserRepository userRepository,
    IOwnerRepository ownerRepository,
    IRenterProviderRepository renterProviderRepository) : ControllerBase
{
    /**
     * <summary>
     *     Sign in endpoint. It allows authenticating a user
     * </summary>
     * <param name="signInResource">The sign-in resource containing username and password.</param>
     * <returns>The authenticated user resource, JWT token, or 2FA setup/verification requirement</returns>
     */
    [HttpPost("sign-in")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Sign in",
        Description = "Sign in a user. May require 2FA setup on first login or 2FA verification if enabled.",
        OperationId = "SignIn")]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was authenticated or needs 2FA setup/verification")]
    public async Task<IActionResult> SignIn([FromBody] SignInResource signInResource)
    {
        try
        {
            var signInCommand = SignInCommandFromResourceAssembler.ToCommandFromResource(signInResource);
            var authenticatedUser = await userCommandService.Handle(signInCommand);

            // If token is empty, check if 2FA setup or verification is required
            if (string.IsNullOrEmpty(authenticatedUser.token))
            {
                var userFor2FA = authenticatedUser.user;

                // First login - needs 2FA setup (secret was just generated)
                if (!userFor2FA.TwoFactorEnabled && !string.IsNullOrEmpty(userFor2FA.TwoFactorSecret))
                {
                    var twoFactorSetup = twoFactorService.GenerateTwoFactorSecret(userFor2FA.Username);

                    return Ok(new
                    {
                        requiresTwoFactorSetup = true,
                        username = userFor2FA.Username,
                        qrCodeDataUrl = twoFactorSetup.QrCodeDataUrl,
                        manualEntryKey = twoFactorSetup.ManualEntryKey,
                        message = "First login detected. Please scan the QR code with Google Authenticator and enter the 6-digit code."
                    });
                }

                // 2FA is enabled - needs verification code
                if (userFor2FA.TwoFactorEnabled)
                {
                    return Ok(new
                    {
                        requires2FA = true,
                        username = userFor2FA.Username,
                        message = "Please enter your 6-digit code from Google Authenticator."
                    });
                }
            }

            // Normal authentication - detect user type and return token + profile info
            var user = authenticatedUser.user;
            var token = authenticatedUser.token;

            // Check if user is an Owner
            var owner = await ownerRepository.FindByUserIdAsync(user.Id);
            if (owner != null)
            {
                return Ok(new
                {
                    id = user.Id,
                    username = user.Username,
                    token,
                    userType = "Owner",
                    profileId = owner.Id,
                    balance = owner.Balance,
                    planId = owner.PlanId,
                    maxUnits = owner.MaxUnits
                });
            }

            // Check if user is a Provider
            var provider = await renterProviderRepository.FindByUserIdAsync(user.Id);
            if (provider != null)
            {
                return Ok(new
                {
                    id = user.Id,
                    username = user.Username,
                    token,
                    userType = "Provider",
                    profileId = provider.Id,
                    balance = provider.Balance,
                    planId = provider.PlanId,
                    maxClients = provider.MaxClients,
                    companyName = provider.CompanyName
                });
            }

            // User has no profile yet - return basic authentication
            var resource =
                AuthenticatedUserResourceFromEntityAssembler.ToResourceFromEntity(user, token);
            return Ok(resource);
        }
        catch (Exception ex)
        {
            return Unauthorized(new
            {
                message = "An error occurred while signing in",
                error = ex.Message
            });
        }
    }

    /**
     * <summary>
     *     Register with payment endpoint - Complete registration with Stripe payment and profile creation
     * </summary>
     * <param name="request">Registration request with payment and profile information</param>
     * <returns>Registration response with generated credentials</returns>
     */
    [HttpPost("register")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Register with payment",
        Description = "Complete user registration with Stripe payment and automatic Owner/Provider profile creation. Payment is processed synchronously and credentials are emailed upon success.",
        OperationId = "RegisterWithPayment")]
    [SwaggerResponse(StatusCodes.Status200OK, "Registration completed successfully", typeof(RegisterWithPaymentResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Registration failed")]
    public async Task<IActionResult> RegisterWithPayment([FromBody] RegisterWithPaymentResource request)
    {
        try
        {
            var response = await registrationService.RegisterWithPaymentAsync(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new RegisterWithPaymentResponse
            {
                Success = false,
                ErrorMessage = $"Registration failed: {ex.Message}"
            });
        }
    }

    /**
     * <summary>
     *     Verify two-factor authentication code
     * </summary>
     * <param name="resource">The verification resource containing username and code</param>
     * <returns>The authenticated user resource with JWT token</returns>
     */
    [HttpPost("verify-2fa")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Verify 2FA code",
        Description = "Verify a 6-digit 2FA code from Google Authenticator. Used for first-time setup or login with 2FA enabled.",
        OperationId = "VerifyTwoFactor")]
    [SwaggerResponse(StatusCodes.Status200OK, "The 2FA code was verified and user is authenticated", typeof(AuthenticatedUserResource))]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorResource resource)
    {
        try
        {
            var command = new VerifyTwoFactorCommand(resource.Username, resource.Code);
            var authenticatedUser = await userCommandService.Handle(command);

            var user = authenticatedUser.user;
            var token = authenticatedUser.token;

            // Check if user is an Owner
            var owner = await ownerRepository.FindByUserIdAsync(user.Id);
            if (owner != null)
            {
                return Ok(new
                {
                    id = user.Id,
                    username = user.Username,
                    token,
                    userType = "Owner",
                    profileId = owner.Id,
                    balance = owner.Balance,
                    planId = owner.PlanId,
                    maxUnits = owner.MaxUnits
                });
            }

            // Check if user is a Provider
            var provider = await renterProviderRepository.FindByUserIdAsync(user.Id);
            if (provider != null)
            {
                return Ok(new
                {
                    id = user.Id,
                    username = user.Username,
                    token,
                    userType = "Provider",
                    profileId = provider.Id,
                    balance = provider.Balance,
                    planId = provider.PlanId,
                    maxClients = provider.MaxClients,
                    companyName = provider.CompanyName
                });
            }

            // User has no profile yet - return basic authentication
            var response = AuthenticatedUserResourceFromEntityAssembler.ToResourceFromEntity(user, token);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = "Failed to verify 2FA code",
                error = ex.Message
            });
        }
    }

    /**
     * <summary>
     *     Initiate two-factor authentication setup
     * </summary>
     * <param name="request">The request containing username</param>
     * <returns>QR code and manual entry key for setting up 2FA</returns>
     */
    [HttpPost("initiate-2fa")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Initiate 2FA setup",
        Description = "Generate QR code and manual entry key to set up or reset two-factor authentication",
        OperationId = "InitiateTwoFactor")]
    [SwaggerResponse(StatusCodes.Status200OK, "2FA setup information generated successfully")]
    public async Task<IActionResult> InitiateTwoFactor([FromBody] UsernameRequest request)
    {
        try
        {
            var user = await userRepository.FindByUsernameAsync(request.Username);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var twoFactorSetup = twoFactorService.GenerateTwoFactorSecret(user.Username);

            return Ok(new
            {
                qrCodeDataUrl = twoFactorSetup.QrCodeDataUrl,
                manualEntryKey = twoFactorSetup.ManualEntryKey,
                message = "Scan the QR code with Google Authenticator or enter the key manually. Then verify with a code to complete setup."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = "Failed to initiate 2FA setup",
                error = ex.Message
            });
        }
    }

    /**
     * <summary>
     *     Enable two-factor authentication
     * </summary>
     * <param name="resource">The verification resource containing username and code</param>
     * <returns>Success message</returns>
     */
    [HttpPost("enable-2fa")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Enable 2FA",
        Description = "Re-enable two-factor authentication from user settings. Requires verifying a code to ensure user still has access to their authenticator.",
        OperationId = "EnableTwoFactor")]
    [SwaggerResponse(StatusCodes.Status200OK, "2FA was enabled successfully")]
    public async Task<IActionResult> EnableTwoFactor([FromBody] VerifyTwoFactorResource resource)
    {
        try
        {
            var command = new EnableTwoFactorCommand(resource.Username, resource.Code);
            await userCommandService.Handle(command);

            return Ok(new { message = "Two-factor authentication enabled successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = "Failed to enable 2FA",
                error = ex.Message
            });
        }
    }

    /**
     * <summary>
     *     Disable two-factor authentication
     * </summary>
     * <param name="request">The request containing username</param>
     * <returns>Success message</returns>
     */
    [HttpPost("disable-2fa")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Disable 2FA",
        Description = "Disable two-factor authentication from user settings. The secret is kept so user can re-enable easily.",
        OperationId = "DisableTwoFactor")]
    [SwaggerResponse(StatusCodes.Status200OK, "2FA was disabled successfully")]
    public async Task<IActionResult> DisableTwoFactor([FromBody] UsernameRequest request)
    {
        try
        {
            var command = new DisableTwoFactorCommand(request.Username);
            await userCommandService.Handle(command);

            return Ok(new { message = "Two-factor authentication disabled successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = "Failed to disable 2FA",
                error = ex.Message
            });
        }
    }

    /**
     * <summary>
     *     Get two-factor authentication status
     * </summary>
     * <param name="username">The username</param>
     * <returns>2FA status information</returns>
     */
    [HttpGet("2fa-status")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Get 2FA status",
        Description = "Get the two-factor authentication status for a user",
        OperationId = "GetTwoFactorStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "2FA status retrieved successfully", typeof(TwoFactorStatusResource))]
    public async Task<IActionResult> GetTwoFactorStatus([FromQuery] string username)
    {
        try
        {
            var user = await userRepository.FindByUsernameAsync(username);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var resource = new TwoFactorStatusResource(
                Username: user.Username,
                TwoFactorEnabled: user.TwoFactorEnabled,
                TwoFactorConfigured: !string.IsNullOrEmpty(user.TwoFactorSecret)
            );

            return Ok(resource);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = "Failed to get 2FA status",
                error = ex.Message
            });
        }
    }
}
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Repositories;
using OsitoPolarPlatform.API.IAM.Domain.Model.Queries;
using OsitoPolarPlatform.API.IAM.Domain.Services;
using OsitoPolarPlatform.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using OsitoPolarPlatform.API.IAM.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.IAM.Interfaces.REST.Transform;
using OsitoPolarPlatform.API.Profiles.Domain.Repositories;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Repositories;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace OsitoPolarPlatform.API.IAM.Interfaces.REST;

/**
 * <summary>
 *     The user's controller
 * </summary>
 * <remarks>
 *     This class is used to handle user requests
 * </remarks>
 */
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available User endpoints")]
public class UsersController(
    IUserQueryService userQueryService,
    IOwnerRepository ownerRepository,
    IRenterProviderRepository providerRepository,
    ISubscriptionRepository subscriptionRepository,
    IEquipmentRepository equipmentRepository,
    IServiceRequestRepository serviceRequestRepository) : ControllerBase
{
    /**
     * <summary>
     *     Get user by id endpoint. It allows to get a user by id with full profile information
     * </summary>
     * <param name="id">The user id</param>
     * <returns>The user resource with profile and subscription data</returns>
     */
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get a user by its id",
        Description = "Get a user by its id with complete profile and subscription information",
        OperationId = "GetUserById")]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was found", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The user was not found")]
    public async Task<IActionResult> GetUserById(int id)
    {
        // Get user from IAM
        var getUserByIdQuery = new GetUserByIdQuery(id);
        var user = await userQueryService.Handle(getUserByIdQuery);

        if (user is null)
            return NotFound(new { message = $"User with id {id} not found" });

        // Try to get Owner profile
        var ownerProfile = await ownerRepository.FindByUserIdAsync(id);
        var ownerSubscription = ownerProfile != null
            ? await subscriptionRepository.FindByIdAsync(ownerProfile.PlanId)
            : null;

        // Try to get Provider profile
        var providerProfile = await providerRepository.FindByUserIdAsync(id);
        var providerSubscription = providerProfile != null
            ? await subscriptionRepository.FindByIdAsync(providerProfile.PlanId)
            : null;

        // Get counts for statistics
        int equipmentCount = 0;
        int activeServiceRequestsCount = 0;
        int clientCount = 0;

        if (ownerProfile != null)
        {
            var equipmentList = await equipmentRepository.FindByOwnerIdAsync(ownerProfile.Id);
            equipmentCount = equipmentList.Count();

            // Count service requests for this owner's equipment
            foreach (var equipment in equipmentList)
            {
                var requests = await serviceRequestRepository.FindByEquipmentIdAsync(equipment.Id);
                activeServiceRequestsCount += requests.Count(sr =>
                    sr.Status.ToString() == "Pending" ||
                    sr.Status.ToString() == "InProgress");
            }
        }

        if (providerProfile != null)
        {
            // Count active service requests assigned to this provider's technicians
            // For now, we'll count all service requests (we can refine this later with provider-specific logic)
            var allRequests = await serviceRequestRepository.ListAsync();
            activeServiceRequestsCount = allRequests.Count(sr =>
                sr.Status.ToString() == "Pending" ||
                sr.Status.ToString() == "InProgress");

            // TODO: Implement client count logic when we have the relationship set up
            // For now, we'll use a placeholder value
            clientCount = 0;
        }

        var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(
            user,
            ownerProfile,
            providerProfile,
            ownerSubscription,
            providerSubscription,
            equipmentCount,
            activeServiceRequestsCount,
            clientCount
        );

        return Ok(userResource);
    }

    /**
     * <summary>
     *     Get all users' endpoint. It allows getting all users (basic info only)
     * </summary>
     * <returns>The user resources with basic profile information</returns>
     */
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all users",
        Description = "Get all users with basic information. Use GET /api/v1/users/{id} for complete profile details.",
        OperationId = "GetAllUsers")]
    [SwaggerResponse(StatusCodes.Status200OK, "The users were found", typeof(IEnumerable<UserResource>))]
    public async Task<IActionResult> GetAllUsers()
    {
        var getAllUsersQuery = new GetAllUsersQuery();
        var users = await userQueryService.Handle(getAllUsersQuery);

        // For list view, return basic info without detailed profile data
        var userResources = users.Select(user => UserResourceFromEntityAssembler.ToResourceFromEntity(user));
        return Ok(userResources);
    }
}
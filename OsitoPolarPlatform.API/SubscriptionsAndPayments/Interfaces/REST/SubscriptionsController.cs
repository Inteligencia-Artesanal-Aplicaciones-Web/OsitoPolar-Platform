using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Queries;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Services;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Commands;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Subscription Management Endpoints")]
public class SubscriptionsController(
    ISubscriptionCommandService subscriptionCommandService,
    ISubscriptionQueryService subscriptionQueryService) : ControllerBase
{
    /// <summary>
    /// Creates a new subscription plan.
    /// </summary>
    /// <param name="resource">The resource containing subscription plan details.</param>
    /// <returns>The created subscription resource.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create Subscription Plan",
        Description = "Creates a new subscription plan in the system.",
        OperationId = "CreateSubscription")]
    [SwaggerResponse(StatusCodes.Status201Created, "Subscription created", typeof(SubscriptionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The subscription could not be created")]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionResource resource)
    {
        try
        {
            var createSubscriptionCommand = CreateSubscriptionCommandFromResourceAssembler.ToCommandFromResource(resource);
            var subscription = await subscriptionCommandService.Handle(createSubscriptionCommand);
            if (subscription is null)
            {
                return BadRequest("The subscription could not be created");
            }
            var createdResource = SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription);
            return CreatedAtAction(nameof(GetSubscriptionById), new { subscriptionId = createdResource.Id }, createdResource);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets all subscriptions for a specific user type.
    /// </summary>
    /// <param name="userType">The type of user (e.g., 'user' or 'provider').</param>
    /// <returns>A list of subscriptions as resources.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get All Subscriptions",
        Description = "Returns a list of all subscriptions for a specific user type.",
        OperationId = "GetAllSubscriptions")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of subscriptions", typeof(IEnumerable<SubscriptionResource>))]
    public async Task<IActionResult> GetAllSubscriptions([FromQuery] string userType)
    {
        var query = new GetPlansQuery(userType);
        var subscriptions = await subscriptionQueryService.Handle(query);
        var resources = subscriptions.Select(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity).ToList();
        return Ok(resources);
    }

    /// <summary>
    /// Gets a subscription by its unique identifier.
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription to retrieve.</param>
    /// <returns>A SubscriptionResource if found, or 404 Not Found if not found.</returns>
    [HttpGet("{subscriptionId:int}")]
    [SwaggerOperation(
        Summary = "Get Subscription by Id",
        Description = "Returns a subscription by its unique identifier.",
        OperationId = "GetSubscriptionById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Subscription found", typeof(SubscriptionResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Subscription not found")]
    public async Task<IActionResult> GetSubscriptionById(int subscriptionId)
    {
        var query = new GetSubscriptionByIdQuery(subscriptionId);
        var subscription = await subscriptionQueryService.Handle(query);
        if (subscription is null) return NotFound();
        var resource = SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription);
        return Ok(resource);
    }

    /// <summary>
    /// Upgrades a subscription to a new plan.
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription to upgrade.</param>
    /// <param name="resource">The resource containing the upgrade details.</param>
    /// <returns>The updated subscription resource with a 201 Created status code, or 400 Bad Request if upgrade failed.</returns>
    [HttpPatch("{subscriptionId:int}")]
    [SwaggerOperation(
        Summary = "Upgrade Subscription",
        Description = "Upgrades a subscription to a new plan.",
        OperationId = "UpgradeSubscription")]
    [SwaggerResponse(StatusCodes.Status201Created, "Subscription upgraded", typeof(SubscriptionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The subscription could not be upgraded")]
    public async Task<IActionResult> UpgradeSubscription(int subscriptionId, [FromBody] UpgradeSubscriptionResource resource)
    {
        try
        {
            var command = UpgradeSubscriptionCommandFromResourceAssembler.ToCommandFromResource(resource with { UserId = subscriptionId });
            var subscription = await subscriptionCommandService.Handle(command);
            if (subscription is null) return BadRequest("Subscription could not be upgraded.");
            var createdResource = SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription);
            return CreatedAtAction(nameof(GetSubscriptionById), new { subscriptionId = createdResource.Id }, createdResource);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a subscription plan.
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription to delete.</param>
    /// <returns>No content if successful, or 404 Not Found if the subscription doesn't exist.</returns>
    [HttpDelete("{subscriptionId:int}")]
    [SwaggerOperation(
        Summary = "Delete Subscription Plan",
        Description = "Deletes a subscription plan from the system.",
        OperationId = "DeleteSubscription")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Subscription deleted successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Subscription not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Subscription could not be deleted")]
    public async Task<IActionResult> DeleteSubscription([FromRoute] int subscriptionId)
    {
        try
        {
            var deleteCommand = new DeleteSubscriptionCommand(subscriptionId);
            var success = await subscriptionCommandService.Handle(deleteCommand);
            if (!success)
            {
                return NotFound("Subscription not found or could not be deleted");
            }
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}

public record UpgradeSubscriptionResource(int UserId, int PlanId);
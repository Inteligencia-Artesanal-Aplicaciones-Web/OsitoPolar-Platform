using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Queries;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Services;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;
//using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Commands; 
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
    /// Upgrades a subscription to a new plan.
    /// </summary>
    /// <param name="resource">The resource containing the upgrade details.</param>
    /// <returns>The updated subscription resource with a 201 Created status code, or 400 Bad Request if upgrade failed.</returns>
    [HttpPost("upgrade")]
    [SwaggerOperation(
        Summary = "Upgrade Subscription",
        Description = "Upgrades a subscription to a new plan.",
        OperationId = "UpgradeSubscription")]
    [SwaggerResponse(StatusCodes.Status201Created, "Subscription upgraded", typeof(SubscriptionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The subscription could not be upgraded")]
    public async Task<IActionResult> UpgradeSubscription([FromBody] UpgradeSubscriptionResource resource)
    {
        try
        {
            var command = UpgradeSubscriptionCommandFromResourceAssembler.ToCommandFromResource(resource);
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
}

public record UpgradeSubscriptionResource(int UserId, int PlanId);
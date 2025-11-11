using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Commands;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Services;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Repositories;
using OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Transform;
using OsitoPolarPlatform.API.IAM.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using OsitoPolarPlatform.API.Profiles.Interfaces.ACL;
using OsitoPolarPlatform.API.EquipmentManagement.Interfaces.ACL;

namespace OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST;

/// <summary>
/// Marketplace for service requests - Uber-style system
/// Owners create requests → Providers see them → First to accept wins
/// </summary>
[ApiController]
[Route("api/v1/service-requests")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Service Request Marketplace (Uber-style)")]
public class ServiceRequestMarketplaceController : ControllerBase
{
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly IServiceRequestCommandService _serviceRequestCommandService;
    private readonly IProfilesContextFacade _profilesFacade;
    private readonly IEquipmentContextFacade _equipmentFacade;

    public ServiceRequestMarketplaceController(
        IServiceRequestRepository serviceRequestRepository,
        IServiceRequestCommandService serviceRequestCommandService,
        IProfilesContextFacade profilesFacade,
        IEquipmentContextFacade equipmentFacade)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _serviceRequestCommandService = serviceRequestCommandService;
        _profilesFacade = profilesFacade;
        _equipmentFacade = equipmentFacade;
    }

    /// <summary>
    /// Get all available service requests in the marketplace (Providers only)
    /// Shows all pending requests that Providers can accept
    /// </summary>
    [Authorize]
    [HttpGet("marketplace")]
    [SwaggerOperation(
        Summary = "Get Available Service Requests (Marketplace)",
        Description = "Returns all pending service requests that Providers can accept. Similar to Uber - shows all available requests waiting for a provider.",
        OperationId = "GetMarketplaceServiceRequests")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of available service requests")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Only Providers can access the marketplace")]
    public async Task<IActionResult> GetMarketplaceServiceRequests()
    {
        try
        {
            // Verify user is a Provider
            var user = (User?)HttpContext.Items["User"];
            if (user == null)
                return Unauthorized(new { message = "User not authenticated" });

            var providerId = await _profilesFacade.FetchProviderIdByUserId(user.Id);
            if (providerId == 0)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Only Providers can access the marketplace" });

            Console.WriteLine($"[Marketplace] Provider {providerId} requesting marketplace");

            // Get all pending service requests
            var pendingRequests = await _serviceRequestRepository.FindByStatusAsync(EServiceRequestStatus.Pending);

            // Enrich with equipment details
            var enrichedRequests = new List<object>();
            foreach (var request in pendingRequests)
            {
                // Equipment details simplified - use equipmentId to fetch full details separately if needed
                var equipmentExists = await _equipmentFacade.EquipmentExists(request.EquipmentId);
                object? equipment = equipmentExists ? new { id = request.EquipmentId } : null;

                enrichedRequests.Add(new
                {
                    id = request.Id,
                    orderNumber = request.OrderNumber,
                    title = request.Title,
                    description = request.Description,
                    issueDetails = request.IssueDetails,
                    requestTime = request.RequestTime,
                    status = request.Status.ToString(),
                    priority = request.Priority.ToString(),
                    urgency = request.Urgency.ToString(),
                    isEmergency = request.IsEmergency,
                    serviceType = request.ServiceType.ToString(),
                    scheduledDate = request.ScheduledDate,
                    timeSlot = request.TimeSlot,
                    serviceAddress = request.ServiceAddress,
                    equipmentId = request.EquipmentId,
                    equipment = equipment // Simplified - contains only {id} - fetch full details separately if needed
                });
            }

            Console.WriteLine($"[Marketplace] Returning {enrichedRequests.Count} pending service requests");
            return Ok(enrichedRequests);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Marketplace] Error: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Provider accepts a service request from the marketplace
    /// First provider to accept gets assigned (atomic operation)
    /// </summary>
    [Authorize]
    [HttpPost("{serviceRequestId:int}/accept")]
    [SwaggerOperation(
        Summary = "Accept Service Request (Provider)",
        Description = "Provider accepts a pending service request. First provider to accept gets assigned. Similar to Uber driver accepting a ride.",
        OperationId = "AcceptServiceRequest")]
    [SwaggerResponse(StatusCodes.Status200OK, "Service request accepted successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Service request already assigned or invalid")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Only Providers can accept service requests")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Service request not found")]
    public async Task<IActionResult> AcceptServiceRequest(int serviceRequestId)
    {
        try
        {
            // Verify user is a Provider
            var user = (User?)HttpContext.Items["User"];
            if (user == null)
                return Unauthorized(new { message = "User not authenticated" });

            var providerId = await _profilesFacade.FetchProviderIdByUserId(user.Id);
            if (providerId == 0)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Only Providers can accept service requests" });

            Console.WriteLine($"[Accept] Provider {providerId} attempting to accept service request {serviceRequestId}");

            // Try to accept the service request (atomic operation)
            var command = new AcceptServiceRequestCommand(serviceRequestId, providerId);
            var serviceRequest = await _serviceRequestCommandService.Handle(command);

            if (serviceRequest == null)
            {
                Console.WriteLine($"[Accept] Service request {serviceRequestId} already accepted by another provider");
                return BadRequest(new
                {
                    success = false,
                    message = "Service request is no longer available (already accepted by another provider)"
                });
            }

            Console.WriteLine($"[Accept] Service request {serviceRequestId} successfully accepted by provider {providerId}");

            var resource = ServiceRequestResourceFromEntityAssembler.ToResourceFromEntity(serviceRequest);
            return Ok(new
            {
                success = true,
                message = "Service request accepted successfully! You can now start working on it.",
                serviceRequest = resource
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Accept] Error: {ex.Message}");
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }
}

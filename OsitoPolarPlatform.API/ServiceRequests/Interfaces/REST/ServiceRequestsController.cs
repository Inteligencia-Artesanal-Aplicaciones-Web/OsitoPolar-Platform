using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Queries;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Services;
using OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Commands;
using OsitoPolarPlatform.API.IAM.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using OsitoPolarPlatform.API.Profiles.Domain.Repositories;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Repositories;

namespace OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST;
/// <summary>
/// REST API controller for managing Service Requests.
/// </summary>
/// <param name="serviceRequestCommandService">The command service for handling service request commands.</param>
/// <param name="serviceRequestQueryService">The query service for handling service request queries.</param>
/// <param name="ownerRepository">Repository for owner data.</param>
/// <param name="equipmentRepository">Repository for equipment data.</param>
///
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Service Request Endpoints")]
public class ServiceRequestsController(
    IServiceRequestCommandService serviceRequestCommandService,
    IServiceRequestQueryService serviceRequestQueryService,
    IOwnerRepository ownerRepository,
    IEquipmentRepository equipmentRepository) : ControllerBase
{
    /// <summary>
    /// Helper method to get the authenticated owner profile
    /// </summary>
    /// <returns>The owner profile or error result</returns>
    private async Task<(ActionResult? error, Profiles.Domain.Model.Aggregates.Owner? owner)> GetAuthenticatedOwner()
    {
        var user = (User?)HttpContext.Items["User"];
        if (user == null)
            return (Unauthorized(new { message = "User not authenticated" }), null);

        var ownerProfile = await ownerRepository.FindByUserIdAsync(user.Id);
        if (ownerProfile == null)
            return (StatusCode(StatusCodes.Status403Forbidden,
                new { message = "User is not an owner. Only owners can manage service requests." }), null);

        return (null, ownerProfile);
    }

    /// <summary>
    /// Creates a new service request in the system.
    /// Service requests can only be created for equipment owned by the authenticated user.
    /// </summary>
    /// <param name="resource">The resource containing the service request details.</param>
    /// <returns>The created service request resource.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create Service Request",
        Description = "Creates a new service request in the system. Can only create requests for your own equipment.",
        OperationId = "CreateServiceRequest")]
    [SwaggerResponse(StatusCodes.Status201Created, "Service Request created", typeof(ServiceRequestResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The service request could not be created")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User not authenticated")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Equipment does not belong to the authenticated owner")]
    public async Task<IActionResult> CreateServiceRequest([FromBody] CreateServiceRequestResource resource)
    {
        // Get authenticated owner
        var (error, ownerProfile) = await GetAuthenticatedOwner();
        if (error != null) return error;

        // Verify equipment ownership
        var equipment = await equipmentRepository.FindByIdAsync(resource.EquipmentId);
        if (equipment == null)
            return NotFound(new { message = "Equipment not found" });

        if (equipment.OwnerId != ownerProfile!.Id)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only create service requests for your own equipment" });

        var createServiceRequestCommand = CreateServiceRequestCommandFromResourceAssembler.ToCommandFromResource(resource);
        var serviceRequest = await serviceRequestCommandService.Handle(createServiceRequestCommand);
        if (serviceRequest is null)
        {
            return BadRequest("The service request could not be created");
        }
        var createdResource = ServiceRequestResourceFromEntityAssembler.ToResourceFromEntity(serviceRequest);
        return CreatedAtAction(nameof(GetServiceRequestById), new { serviceRequestId = createdResource.Id }, createdResource);
    }

    
    /// <summary>
    /// Updates an existing service request in the system.
    /// </summary>
    /// <param name="serviceRequestId">The ID of the service request to update.</param>
    /// <param name="resource">The resource containing the updated details of the service request.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the updated service request resource, or a 404 Not Found if the request does not exist.
    /// </returns>
    [HttpPut("{serviceRequestId:int}")]
    [SwaggerOperation(
        Summary = "Update an existing Service Request",
        Description = "Updates an existing service request in the system. Can only update your own service requests.",
        OperationId = "UpdateServiceRequest")]
    [SwaggerResponse(StatusCodes.Status200OK, "Service Request updated", typeof(ServiceRequestResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Service Request not found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Service request does not belong to the authenticated owner")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Failed to update service request")]
    public async Task<IActionResult> UpdateServiceRequest([FromRoute] int serviceRequestId, [FromBody] UpdateServiceRequestResource resource)
    {
        // Get authenticated owner
        var (error, ownerProfile) = await GetAuthenticatedOwner();
        if (error != null) return error;

        // Get service request and verify ownership
        var serviceRequest = await serviceRequestQueryService.Handle(new GetServiceRequestByIdQuery(serviceRequestId));
        if (serviceRequest is null)
            return NotFound("Service Request not found");

        var equipment = await equipmentRepository.FindByIdAsync(serviceRequest.EquipmentId);
        if (equipment == null || equipment.OwnerId != ownerProfile!.Id)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You don't have permission to update this service request" });

        var updateServiceRequestCommand = UpdateServiceRequestCommandFromResourceAssembler.ToCommandFromResource(serviceRequestId, resource);
        var updatedServiceRequest = await serviceRequestCommandService.Handle(updateServiceRequestCommand);
        if (updatedServiceRequest is null) return NotFound("Service Request not found or could not be updated.");
        var updatedResource = ServiceRequestResourceFromEntityAssembler.ToResourceFromEntity(updatedServiceRequest);
        return Ok(updatedResource);
    }
    
    /// <summary>
    /// Gets all service requests for the authenticated owner's equipment.
    /// </summary>
    /// <returns>A list of service requests for the authenticated owner's equipment only.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get All Service Requests",
        Description = "Returns a list of all service requests for the authenticated owner's equipment only.",
        OperationId = "GetAllServiceRequests")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of service requests", typeof(IEnumerable<ServiceRequestResource>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User not authenticated")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not an owner")]
    public async Task<IActionResult> GetAllServiceRequests()
    {
        // Get authenticated owner
        var (error, ownerProfile) = await GetAuthenticatedOwner();
        if (error != null) return error;

        // Get all equipment owned by this owner
        var ownerEquipment = await equipmentRepository.FindByOwnerIdAsync(ownerProfile!.Id);
        var equipmentIds = ownerEquipment.Select(e => e.Id).ToHashSet();

        // Get all service requests and filter by owner's equipment
        var getAllServiceRequestsQuery = new GetAllServiceRequestsQuery();
        var allServiceRequests = await serviceRequestQueryService.Handle(getAllServiceRequestsQuery);
        var ownerServiceRequests = allServiceRequests.Where(sr => equipmentIds.Contains(sr.EquipmentId));

        var resources = ownerServiceRequests.Select(ServiceRequestResourceFromEntityAssembler.ToResourceFromEntity).ToList();
        return Ok(resources);
    }

    /// <summary>
    /// Gets a service request by its unique identifier.
    /// </summary>
    /// <param name="serviceRequestId">The ID of the service request to retrieve.</param>
    /// <returns>The requested service request resource, if found.</returns>
    [HttpGet("{serviceRequestId:int}")]
    [SwaggerOperation(
        Summary = "Get Service Request by Id",
        Description = "Returns a service request by its unique identifier if it belongs to the authenticated owner.",
        OperationId = "GetServiceRequestById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Service Request found", typeof(ServiceRequestResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Service Request not found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Service request does not belong to the authenticated owner")]
    public async Task<IActionResult> GetServiceRequestById(int serviceRequestId)
    {
        // Get authenticated owner
        var (error, ownerProfile) = await GetAuthenticatedOwner();
        if (error != null) return error;

        var getServiceRequestByIdQuery = new GetServiceRequestByIdQuery(serviceRequestId);
        var serviceRequest = await serviceRequestQueryService.Handle(getServiceRequestByIdQuery);
        if (serviceRequest is null)
        {
            return NotFound();
        }

        // Verify ownership via equipment
        var equipment = await equipmentRepository.FindByIdAsync(serviceRequest.EquipmentId);
        if (equipment == null || equipment.OwnerId != ownerProfile!.Id)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You don't have permission to view this service request" });

        var resource = ServiceRequestResourceFromEntityAssembler.ToResourceFromEntity(serviceRequest);
        return Ok(resource);
    }

    /// <summary>
    /// Assigns a technician to a service request and updates its status.
    /// Note: This should typically be done by service providers, not owners.
    /// </summary>
    /// <param name="serviceRequestId">The ID of the service request.</param>
    /// <param name="resource">The resource containing the technician ID.</param>
    /// <returns>The updated service request resource.</returns>
    [HttpPut("{serviceRequestId:int}/technician")]
    [SwaggerOperation(
        Summary = "Assign Technician to Service Request",
        Description = "Assigns a technician to a service request, updating its status to Accepted and creating a Work Order. Requires ownership of the equipment.",
        OperationId = "AssignTechnicianToServiceRequest")]
    [SwaggerResponse(StatusCodes.Status200OK, "Technician assigned successfully", typeof(ServiceRequestResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request or technician already assigned")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Service Request not found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Service request does not belong to the authenticated owner")]
    public async Task<IActionResult> AssignTechnician(int serviceRequestId, [FromBody] AssignTechnicianToServiceRequestResource resource)
    {
        // Get authenticated owner
        var (error, ownerProfile) = await GetAuthenticatedOwner();
        if (error != null) return error;

        // Verify ownership
        var serviceRequest = await serviceRequestQueryService.Handle(new GetServiceRequestByIdQuery(serviceRequestId));
        if (serviceRequest == null)
            return NotFound();

        var equipment = await equipmentRepository.FindByIdAsync(serviceRequest.EquipmentId);
        if (equipment == null || equipment.OwnerId != ownerProfile!.Id)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You don't have permission to modify this service request" });

        var command = AssignTechnicianToServiceRequestCommandFromResourceAssembler.ToCommandFromResource(serviceRequestId, resource);
        var updatedServiceRequest = await serviceRequestCommandService.Handle(command);
        if (updatedServiceRequest == null)
        {
            return NotFound();
        }
        var updatedResource = ServiceRequestResourceFromEntityAssembler.ToResourceFromEntity(updatedServiceRequest);
        return Ok(updatedResource);
    }

    /// <summary>
    /// Adds customer feedback to a resolved service request.
    /// </summary>
    /// <param name="serviceRequestId">The ID of the service request.</param>
    /// <param name="resource">The resource containing the rating.</param>
    /// <returns>The updated service request resource.</returns>
    [HttpPut("{serviceRequestId:int}/feedback")]
    [SwaggerOperation(
        Summary = "Add Customer Feedback to Service Request",
        Description = "Adds a customer feedback rating (1-5) to a resolved service request. Only the owner can add feedback.",
        OperationId = "AddCustomerFeedbackToServiceRequest")]
    [SwaggerResponse(StatusCodes.Status200OK, "Feedback added successfully", typeof(ServiceRequestResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid rating or service request not resolved")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Service Request not found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Service request does not belong to the authenticated owner")]
    public async Task<IActionResult> AddCustomerFeedback(int serviceRequestId, [FromBody] AddCustomerFeedbackToServiceRequestResource resource)
    {
        // Get authenticated owner
        var (error, ownerProfile) = await GetAuthenticatedOwner();
        if (error != null) return error;

        // Verify ownership
        var serviceRequest = await serviceRequestQueryService.Handle(new GetServiceRequestByIdQuery(serviceRequestId));
        if (serviceRequest == null)
            return NotFound();

        var equipment = await equipmentRepository.FindByIdAsync(serviceRequest.EquipmentId);
        if (equipment == null || equipment.OwnerId != ownerProfile!.Id)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You don't have permission to add feedback to this service request" });

        var command = AddCustomerFeedbackToServiceRequestCommandFromResourceAssembler.ToCommandFromResource(serviceRequestId, resource);
        var updatedServiceRequest = await serviceRequestCommandService.Handle(command);
        if (updatedServiceRequest == null)
        {
            return NotFound();
        }
        var updatedResource = ServiceRequestResourceFromEntityAssembler.ToResourceFromEntity(updatedServiceRequest);
        return Ok(updatedResource);
    }

    /// <summary>
    /// Updates the status of a service request (reject or cancel).
    /// </summary>
    /// <param name="serviceRequestId">The ID of the service request to update.</param>
    /// <param name="resource">The resource containing the new status (e.g., 'rejected', 'cancelled').</param>
    /// <returns>The updated service request resource.</returns>
    [HttpPut("{serviceRequestId:int}/status")]
    [SwaggerOperation(
        Summary = "Update Service Request Status (Reject/Cancel)",
        Description = "Updates the status of a service request to 'rejected' or 'cancelled'. Only the owner can update status.",
        OperationId = "UpdateServiceRequestStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "Service Request status updated successfully", typeof(ServiceRequestResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid status or transition")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Service Request not found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Service request does not belong to the authenticated owner")]
    public async Task<IActionResult> UpdateServiceRequestStatus(int serviceRequestId, [FromBody] UpdateServiceRequestStatusResource resource)
    {
        // Get authenticated owner
        var (error, ownerProfile) = await GetAuthenticatedOwner();
        if (error != null) return error;

        // Verify ownership
        var existingServiceRequest = await serviceRequestQueryService.Handle(new GetServiceRequestByIdQuery(serviceRequestId));
        if (existingServiceRequest == null)
            return NotFound();

        var equipment = await equipmentRepository.FindByIdAsync(existingServiceRequest.EquipmentId);
        if (equipment == null || equipment.OwnerId != ownerProfile!.Id)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You don't have permission to update this service request" });

        var status = resource?.NewStatus;
        if (string.IsNullOrWhiteSpace(status))
            return BadRequest("Status is required. Use 'rejected' or 'cancelled'.");

        if (status.Equals("rejected", StringComparison.OrdinalIgnoreCase))
        {
            var command = new RejectServiceRequestCommand(serviceRequestId);
            var serviceRequest = await serviceRequestCommandService.Handle(command);
            if (serviceRequest == null)
                return NotFound();
            var updatedResource = ServiceRequestResourceFromEntityAssembler.ToResourceFromEntity(serviceRequest);
            return Ok(updatedResource);
        }
        else if (status.Equals("cancelled", StringComparison.OrdinalIgnoreCase) || status.Equals("canceled", StringComparison.OrdinalIgnoreCase))
        {
            var command = new CancelServiceRequestCommand(serviceRequestId);
            var serviceRequest = await serviceRequestCommandService.Handle(command);
            if (serviceRequest == null)
                return NotFound();
            var updatedResource = ServiceRequestResourceFromEntityAssembler.ToResourceFromEntity(serviceRequest);
            return Ok(updatedResource);
        }
        else
        {
            return BadRequest("Invalid status value. Use 'rejected' or 'cancelled'.");
        }
    }
}
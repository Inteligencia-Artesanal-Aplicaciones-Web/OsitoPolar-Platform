using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Queries;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Services;
using OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Commands; 

namespace OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST;
/// <summary>
/// REST API controller for managing Service Requests.
/// </summary>
/// <param name="serviceRequestCommandService">The command service for handling service request commands.</param>
/// <param name="serviceRequestQueryService">The query service for handling service request queries.</param>
/// 
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Service Request Endpoints")]
public class ServiceRequestsController(
    IServiceRequestCommandService serviceRequestCommandService,
    IServiceRequestQueryService serviceRequestQueryService) : ControllerBase
{
    /// <summary>
    /// Creates a new service request in the system.
    /// </summary>
    /// <param name="resource">The resource containing the service request details.</param>
    /// <returns>The created service request resource.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create Service Request",
        Description = "Creates a new service request in the system.",
        OperationId = "CreateServiceRequest")]
    [SwaggerResponse(StatusCodes.Status201Created, "Service Request created", typeof(ServiceRequestResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The service request could not be created")]
    public async Task<IActionResult> CreateServiceRequest([FromBody] CreateServiceRequestResource resource)
    {
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
        Description = "Updates an existing service request in the system.",
        OperationId = "UpdateServiceRequest")]
    [SwaggerResponse(StatusCodes.Status200OK, "Service Request updated", typeof(ServiceRequestResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Service Request not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Failed to update service request")]
    
    public async Task<IActionResult> UpdateServiceRequest([FromRoute] int serviceRequestId, [FromBody] UpdateServiceRequestResource resource)
    {
        var updateServiceRequestCommand = UpdateServiceRequestCommandFromResourceAssembler.ToCommandFromResource(serviceRequestId, resource);
        var updatedServiceRequest = await serviceRequestCommandService.Handle(updateServiceRequestCommand);
        if (updatedServiceRequest is null) return NotFound("Service Request not found or could not be updated.");
        var updatedResource = ServiceRequestResourceFromEntityAssembler.ToResourceFromEntity(updatedServiceRequest);
        return Ok(updatedResource);
    }
    
    /// <summary>
    /// Gets all service requests in the system.
    /// </summary>
    /// <returns>A list of all service requests.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get All Service Requests",
        Description = "Returns a list of all service requests in the system.",
        OperationId = "GetAllServiceRequests")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of service requests", typeof(IEnumerable<ServiceRequestResource>))]
    public async Task<IActionResult> GetAllServiceRequests()
    {
        var getAllServiceRequestsQuery = new GetAllServiceRequestsQuery();
        var serviceRequests = await serviceRequestQueryService.Handle(getAllServiceRequestsQuery);
        var resources = serviceRequests.Select(ServiceRequestResourceFromEntityAssembler.ToResourceFromEntity).ToList();
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
        Description = "Returns a service request by its unique identifier.",
        OperationId = "GetServiceRequestById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Service Request found", typeof(ServiceRequestResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Service Request not found")]
    public async Task<IActionResult> GetServiceRequestById(int serviceRequestId)
    {
        var getServiceRequestByIdQuery = new GetServiceRequestByIdQuery(serviceRequestId);
        var serviceRequest = await serviceRequestQueryService.Handle(getServiceRequestByIdQuery);
        if (serviceRequest is null)
        {
            return NotFound();
        }
        var resource = ServiceRequestResourceFromEntityAssembler.ToResourceFromEntity(serviceRequest);
        return Ok(resource);
    }

    /// <summary>
    /// Assigns a technician to a service request and updates its status.
    /// </summary>
    /// <param name="serviceRequestId">The ID of the service request.</param>
    /// <param name="resource">The resource containing the technician ID.</param>
    /// <returns>The updated service request resource.</returns>
    [HttpPut("{serviceRequestId:int}/technician")]
    [SwaggerOperation(
        Summary = "Assign Technician to Service Request",
        Description = "Assigns a technician to a service request, updating its status to Accepted and creating a Work Order.",
        OperationId = "AssignTechnicianToServiceRequest")]
    [SwaggerResponse(StatusCodes.Status200OK, "Technician assigned successfully", typeof(ServiceRequestResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request or technician already assigned")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Service Request not found")]
    public async Task<IActionResult> AssignTechnician([FromRoute] int serviceRequestId, [FromBody] AssignTechnicianToServiceRequestResource resource)  
    {
        var command = AssignTechnicianToServiceRequestCommandFromResourceAssembler.ToCommandFromResource(serviceRequestId, resource);
        var serviceRequest = await serviceRequestCommandService.Handle(command);
        if (serviceRequest == null)
        {
            return NotFound();
        }
        var updatedResource = ServiceRequestResourceFromEntityAssembler.ToResourceFromEntity(serviceRequest);
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
        Description = "Adds a customer feedback rating (1-5) to a resolved service request.",
        OperationId = "AddCustomerFeedbackToServiceRequest")]
    [SwaggerResponse(StatusCodes.Status200OK, "Feedback added successfully", typeof(ServiceRequestResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid rating or service request not resolved")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Service Request not found")]
    public async Task<IActionResult> AddCustomerFeedback([FromRoute] int serviceRequestId, [FromBody] AddCustomerFeedbackToServiceRequestResource resource)
    {
        var command = AddCustomerFeedbackToServiceRequestCommandFromResourceAssembler.ToCommandFromResource(serviceRequestId, resource);
        var serviceRequest = await serviceRequestCommandService.Handle(command);
        if (serviceRequest == null)
        {
            return NotFound();
        }
        var updatedResource = ServiceRequestResourceFromEntityAssembler.ToResourceFromEntity(serviceRequest);
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
        Description = "Updates the status of a service request to 'rejected' or 'cancelled'.",
        OperationId = "UpdateServiceRequestStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "Service Request status updated successfully", typeof(ServiceRequestResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid status or transition")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Service Request not found")]
    public async Task<IActionResult> UpdateServiceRequestStatus(int serviceRequestId, [FromBody] UpdateServiceRequestStatusResource resource)
    {
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
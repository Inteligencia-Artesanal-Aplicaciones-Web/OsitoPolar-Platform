using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Commands;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Queries;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Services;
using OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace OsitoPolarPlatform.API.ServiceRequests.Interfaces.REST;

/// <summary>
/// REST API controller for managing Service Requests.
/// </summary>
/// <param name="serviceRequestCommandService">The command service for handling service request commands.</param>
/// <param name="serviceRequestQueryService">The query service for handling service request queries.</param>
[ApiController] 
[Route("api/v1/[controller]")] 
[Produces(MediaTypeNames.Application.Json)] 
[SwaggerTag("Available Service Request Endpoints")] 
public class ServiceRequestsController(
    IServiceRequestCommandService serviceRequestCommandService,
    IServiceRequestQueryService serviceRequestQueryService) : ControllerBase 
{
    /// <summary>
    /// Gets a service request by its unique identifier.
    /// </summary>
    /// <param name="serviceRequestId">The unique identifier of the service request to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the service request resource if found, or a 404 Not Found response if not found.
    /// </returns>
    [HttpGet("{serviceRequestId:int}")] 
    [SwaggerOperation(
        Summary = "Get Service Request by Id",
        Description = "Returns a service request by its unique identifier.",
        OperationId = "GetServiceRequestById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Service Request found", typeof(ServiceRequestResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Service Request not found")]
    public async Task<IActionResult> GetServiceRequestById([FromRoute] int serviceRequestId) 
    {
        var getServiceRequestByIdQuery = new GetServiceRequestByIdQuery(serviceRequestId);
        var serviceRequest = await serviceRequestQueryService.Handle(getServiceRequestByIdQuery);
        if (serviceRequest is null) return NotFound();
        var resource = ServiceRequestResourceFromEntityAssembler.ToResourceFromEntity(serviceRequest);
        return Ok(resource);
    }

    /// <summary>
    /// Creates a new service request in the system.
    /// </summary>
    /// <param name="resource">The resource containing the details of the service request to create.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the created service request resource with a 201 Created status code, or a 400 Bad Request if the creation failed.
    /// </returns>
    [HttpPost] // Define una ruta POST para la creación
    [SwaggerOperation(
        Summary = "Create a new Service Request",
        Description = "Creates a new service request in the system.",
        OperationId = "CreateServiceRequest")]
    [SwaggerResponse(StatusCodes.Status201Created, "Service Request created", typeof(ServiceRequestResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Failed to create service request")]
    public async Task<IActionResult> CreateServiceRequest([FromBody] CreateServiceRequestResource resource) 
    {
        var createServiceRequestCommand = CreateServiceRequestCommandFromResourceAssembler.ToCommandFromResource(resource);
        var serviceRequest = await serviceRequestCommandService.Handle(createServiceRequestCommand);
        if (serviceRequest is null) return BadRequest("Failed to create service request.");
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
    /// <returns>
    /// An <see cref="IActionResult"/> containing a list of all service requests as resources.
    /// </returns>
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
    /// Assigns a technician to a service request.
    /// </summary>
    /// <param name="serviceRequestId">The ID of the service request to assign a technician to.</param>
    /// <param name="resource">The resource containing the ID of the technician to assign.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the updated service request resource, or a 404 Not Found if the request does not exist.
    /// </returns>
    [HttpPut("{serviceRequestId:int}/assign-technician")]
    [SwaggerOperation(
        Summary = "Assign Technician to Service Request",
        Description = "Assigns a technician to an existing service request.",
        OperationId = "AssignTechnicianToServiceRequest")]
    [SwaggerResponse(StatusCodes.Status200OK, "Technician assigned", typeof(ServiceRequestResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Service Request or Technician not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Failed to assign technician")]
    public async Task<IActionResult> AssignTechnicianToServiceRequest([FromRoute] int serviceRequestId, [FromBody] AssignTechnicianToServiceRequestResource resource)
    {
        var assignCommand = new AssignTechnicianToServiceRequestCommand(serviceRequestId, resource.TechnicianId);
        var updatedServiceRequest = await serviceRequestCommandService.Handle(assignCommand);
        if (updatedServiceRequest is null) return NotFound("Service Request or Technician not found.");
        var updatedResource = ServiceRequestResourceFromEntityAssembler.ToResourceFromEntity(updatedServiceRequest);
        return Ok(updatedResource);
    }

    /// <summary>
    /// Updates the status of a service request.
    /// </summary>
    /// <param name="serviceRequestId">The ID of the service request to update.</param>
    /// <param name="resource">The resource containing the new status.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the updated service request resource, or a 404 Not Found if the request does not exist.
    /// </returns>
    [HttpPut("{serviceRequestId:int}/status")]
    [SwaggerOperation(
        Summary = "Update Service Request Status",
        Description = "Updates the status of an existing service request.",
        OperationId = "UpdateServiceRequestStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "Service Request status updated", typeof(ServiceRequestResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Service Request not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid status or failed to update status")]
    public async Task<IActionResult> UpdateServiceRequestStatus([FromRoute] int serviceRequestId, [FromBody] UpdateServiceRequestStatusResource resource)
    {
        if (!Enum.TryParse(resource.NewStatus, true, out EServiceRequestStatus newStatusEnum))
        {
            return BadRequest("Invalid status value provided.");
        }
        var updateStatusCommand = new UpdateServiceRequestStatusCommand(serviceRequestId, newStatusEnum);
        var updatedServiceRequest = await serviceRequestCommandService.Handle(updateStatusCommand);
        if (updatedServiceRequest is null) return NotFound("Service Request not found.");
        var updatedResource = ServiceRequestResourceFromEntityAssembler.ToResourceFromEntity(updatedServiceRequest);
        return Ok(updatedResource);
    }
    
    /// <summary>
    /// Adds customer feedback to a service request.
    /// </summary>
    /// <param name="serviceRequestId">The ID of the service request.</param>
    /// <param name="resource">The customer feedback details.</param>
    /// <returns>The updated service request resource.</returns>
    [HttpPut("{serviceRequestId:int}/customer-feedback")]
    [SwaggerOperation(
        Summary = "Add Customer Feedback to Service Request",
        Description = "Adds customer feedback rating to a service request.",
        OperationId = "AddCustomerFeedback")]
    [SwaggerResponse(StatusCodes.Status200OK, "Customer feedback added", typeof(ServiceRequestResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Service Request not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Failed to add customer feedback")]
    public async Task<IActionResult> AddCustomerFeedbackToServiceRequest([FromRoute] int serviceRequestId, [FromBody] AddCustomerFeedbackToServiceRequestResource resource)
    {
        var addFeedbackCommand = new AddCustomerFeedbackToServiceRequestCommand(
            serviceRequestId,
            resource.Rating
        );
        var updatedServiceRequest = await serviceRequestCommandService.Handle(addFeedbackCommand);
        if (updatedServiceRequest is null) return NotFound("Service Request not found.");
        var updatedResource = ServiceRequestResourceFromEntityAssembler.ToResourceFromEntity(updatedServiceRequest);
        return Ok(updatedResource);
    }
}
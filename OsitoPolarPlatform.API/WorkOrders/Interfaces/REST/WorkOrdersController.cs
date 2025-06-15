using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Commands;
using OsitoPolarPlatform.API.WorkOrders.Domain.Services; 
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Queries; 
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.ValueObjects; 
using OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Swashbuckle.AspNetCore.Annotations;

namespace OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Work Order Endpoints")]
public class WorkOrdersController( 
    IWorkOrderCommandService workOrderCommandService,
    IWorkOrderQueryService workOrderQueryService) : ControllerBase 
{
    /// <summary>
    /// Creates a new Work Order. Can be created manually or from a Service Request.
    /// </summary>
    /// <param name="resource">The resource containing Work Order creation details.</param>
    /// <returns>The created Work Order resource.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new Work Order",
        Description = "Creates a new work order, optionally from an existing service request.",
        OperationId = "CreateWorkOrder")]
    [SwaggerResponse(StatusCodes.Status201Created, "Work Order created", typeof(WorkOrderResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input or failed to create Work Order")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Work Order already exists for Service Request")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "An unexpected error occurred")]
    public async Task<IActionResult> CreateWorkOrder([FromBody] CreateWorkOrderResource resource)
    {
        var command = CreateWorkOrderCommandFromResourceAssembler.ToCommandFromResource(resource);
        try
        {
            var workOrder = await workOrderCommandService.Handle(command); 
            var workOrderResource = WorkOrderResourceFromEntityAssembler.ToResourceFromEntity(workOrder);
            return StatusCode(201, workOrderResource); 
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message }); 
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error creating Work Order: {ex.Message} - {ex.StackTrace}");
            return StatusCode(500, new { message = "An error occurred while creating the Work Order.", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a Work Order by its ID.
    /// </summary>
    /// <param name="workOrderId">The ID of the Work Order.</param>
    /// <returns>The Work Order resource.</returns>
    [HttpGet("{workOrderId:int}")]
    [SwaggerOperation(
        Summary = "Get Work Order by Id",
        Description = "Returns a work order by its unique identifier.",
        OperationId = "GetWorkOrderById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Work Order found", typeof(WorkOrderResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Work Order not found")]
    public async Task<IActionResult> GetWorkOrderById([FromRoute] int workOrderId)
    {
        var query = new GetWorkOrderByIdQuery(workOrderId);
        var workOrder = await workOrderQueryService.Handle(query); 
        if (workOrder == null)
        {
            return NotFound();
        }
        var workOrderResource = WorkOrderResourceFromEntityAssembler.ToResourceFromEntity(workOrder);
        return Ok(workOrderResource);
    }

    /// <summary>
    /// Gets all Work Orders.
    /// </summary>
    /// <returns>A list of Work Order resources.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get All Work Orders",
        Description = "Returns a list of all work orders in the system.",
        OperationId = "GetAllWorkOrders")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of Work Orders", typeof(IEnumerable<WorkOrderResource>))]
    public async Task<IActionResult> GetAllWorkOrders()
    {
        var query = new GetAllWorkOrdersQuery();
        var workOrders = await workOrderQueryService.Handle(query); 
        var workOrderResources = workOrders.Select(WorkOrderResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(workOrderResources);
    }

    /// <summary>
    /// Updates the status of a Work Order.
    /// </summary>
    /// <param name="workOrderId">The ID of the Work Order.</param>
    /// <param name="resource">The resource containing the new status (as string).</param>
    /// <returns>The updated Work Order resource.</returns>
    [HttpPut("{workOrderId:int}/status")]
    [SwaggerOperation(
        Summary = "Update Work Order Status",
        Description = "Updates the status of an existing work order.",
        OperationId = "UpdateWorkOrderStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "Work Order status updated", typeof(WorkOrderResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Work Order not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid status or failed to update status")]
    public async Task<IActionResult> UpdateWorkOrderStatus(int workOrderId, [FromBody] UpdateWorkOrderStatusResource resource)
    {
        if (!Enum.TryParse(resource.NewStatus, true, out EWorkOrderStatus newStatusEnum))
        {
            return BadRequest("Invalid status value provided.");
        }

        var command = new UpdateWorkOrderStatusCommand(workOrderId, newStatusEnum);
        try
        {
            var workOrder = await workOrderCommandService.Handle(command); 
            if (workOrder == null) return NotFound("Work Order not found.");
            var workOrderResource = WorkOrderResourceFromEntityAssembler.ToResourceFromEntity(workOrder);
            return Ok(workOrderResource);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error updating Work Order status: {ex.Message} - {ex.StackTrace}");
            return StatusCode(500, new { message = "An error occurred while updating the Work Order status.", error = ex.Message });
        }
    }

    /// <summary>
    /// Assigns a technician to a Work Order.
    /// </summary>
    /// <param name="workOrderId">The ID of the Work Order.</param>
    /// <param name="resource">The resource containing the technician ID.</param>
    /// <returns>The updated Work Order resource.</returns>
    [HttpPut("{workOrderId:int}/assign-technician")]
    [SwaggerOperation(
        Summary = "Assign Technician to Work Order",
        Description = "Assigns a technician to an existing work order.",
        OperationId = "AssignTechnicianToWorkOrder")]
    [SwaggerResponse(StatusCodes.Status200OK, "Technician assigned", typeof(WorkOrderResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Work Order not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid technician ID or failed to assign")]
    public async Task<IActionResult> AssignTechnicianToWorkOrder(int workOrderId, [FromBody] AssignTechnicianResource resource)
    {
        var command = new AssignTechnicianToWorkOrderCommand(workOrderId, resource.TechnicianId);
        try
        {
            var workOrder = await workOrderCommandService.Handle(command); 
            if (workOrder == null) return NotFound("Work Order not found.");
            var workOrderResource = WorkOrderResourceFromEntityAssembler.ToResourceFromEntity(workOrder);
            return Ok(workOrderResource);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error assigning technician: {ex.Message} - {ex.StackTrace}");
            return StatusCode(500, new { message = "An error occurred while assigning the technician.", error = ex.Message });
        }
    }

    /// <summary>
    /// Adds resolution details to a Work Order.
    /// </summary>
    /// <param name="workOrderId">The ID of the Work Order.</param>
    /// <param name="resource">The resource containing resolution details.</param>
    /// <returns>The updated Work Order resource.</returns>
    [HttpPut("{workOrderId:int}/resolution")]
    [SwaggerOperation(
        Summary = "Add Work Order Resolution Details",
        Description = "Adds resolution details and marks the work order as resolved.",
        OperationId = "AddWorkOrderResolutionDetails")]
    [SwaggerResponse(StatusCodes.Status200OK, "Resolution details added", typeof(WorkOrderResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Work Order not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input or failed to add resolution")]
    public async Task<IActionResult> AddWorkOrderResolutionDetails(int workOrderId, [FromBody] AddWorkOrderResolutionDetailsCommand resource)
    {
        try
        {
            var command = new AddWorkOrderResolutionDetailsCommand(
                workOrderId,
                resource.ResolutionDetails,
                resource.TechnicianNotes,
                resource.Cost
            );
            var workOrder = await workOrderCommandService.Handle(command); 
            if (workOrder == null) return NotFound("Work Order not found.");
            var workOrderResource = WorkOrderResourceFromEntityAssembler.ToResourceFromEntity(workOrder);
            return Ok(workOrderResource);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error adding resolution details: {ex.Message} - {ex.StackTrace}");
            return StatusCode(500, new { message = "An error occurred while adding resolution details.", error = ex.Message });
        }
    }

    /// <summary>
    /// Adds customer feedback (rating and optional comment) to a Work Order.
    /// </summary>
    /// <param name="workOrderId">The ID of the Work Order.</param>
    /// <param name="resource">The resource containing customer feedback.</param>
    /// <returns>The updated Work Order resource.</returns>
    [HttpPut("{workOrderId:int}/feedback")]
    [SwaggerOperation(
        Summary = "Add Customer Feedback to Work Order",
        Description = "Adds customer feedback (rating and optional comment) to a work order.",
        OperationId = "AddWorkOrderCustomerFeedback")]
    [SwaggerResponse(StatusCodes.Status200OK, "Customer feedback added", typeof(WorkOrderResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Work Order not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid feedback or failed to add feedback")]
    public async Task<IActionResult> AddCustomerFeedback(int workOrderId, [FromBody] AddCustomerFeedbackResource resource)
    {
        try
        {
            var command = AddWorkOrderCustomerFeedbackCommandFromResourceAssembler.ToCommandFromResource(workOrderId, resource);
            var workOrder = await workOrderCommandService.Handle(command); 
            if (workOrder == null) return NotFound("Work Order not found.");
            var workOrderResource = WorkOrderResourceFromEntityAssembler.ToResourceFromEntity(workOrder);
            return Ok(workOrderResource);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error adding customer feedback: {ex.Message} - {ex.StackTrace}");
            return StatusCode(500, new { message = "An error occurred while adding customer feedback.", error = ex.Message });
        }
    }
}
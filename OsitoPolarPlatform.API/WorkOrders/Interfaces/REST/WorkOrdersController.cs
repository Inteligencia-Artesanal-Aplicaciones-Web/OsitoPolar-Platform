using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Queries;
using OsitoPolarPlatform.API.WorkOrders.Domain.Services;
using OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Commands; 

namespace OsitoPolarPlatform.API.WorkOrders.Interfaces.REST;

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
        Summary = "Create Work Order",
        Description = "Creates a new work order in the system.",
        OperationId = "CreateWorkOrder")]
    [SwaggerResponse(StatusCodes.Status201Created, "Work Order created", typeof(WorkOrderResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The work order could not be created")]
    public async Task<IActionResult> CreateWorkOrder([FromBody] CreateWorkOrderResource resource)
    {
        var createWorkOrderCommand = CreateWorkOrderCommandFromResourceAssembler.ToCommandFromResource(resource);
        var workOrder = await workOrderCommandService.Handle(createWorkOrderCommand);
        if (workOrder is null)
        {
            return BadRequest("The work order could not be created");
        }
        var createdResource = WorkOrderResourceFromEntityAssembler.ToResourceFromEntity(workOrder);
        return CreatedAtAction(nameof(GetWorkOrderById), new { workOrderId = createdResource.Id }, createdResource);
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
    [SwaggerResponse(StatusCodes.Status200OK, "List of work orders", typeof(IEnumerable<WorkOrderResource>))]
    public async Task<IActionResult> GetAllWorkOrders()
    {
        var getAllWorkOrdersQuery = new GetAllWorkOrdersQuery();
        var workOrders = await workOrderQueryService.Handle(getAllWorkOrdersQuery);
        var resources = workOrders.Select(WorkOrderResourceFromEntityAssembler.ToResourceFromEntity).ToList();
        return Ok(resources);
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
    public async Task<IActionResult> GetWorkOrderById(int workOrderId)
    {
        var getWorkOrderByIdQuery = new GetWorkOrderByIdQuery(workOrderId);
        var workOrder = await workOrderQueryService.Handle(getWorkOrderByIdQuery);
        if (workOrder is null)
        {
            return NotFound();
        }
        var resource = WorkOrderResourceFromEntityAssembler.ToResourceFromEntity(workOrder);
        return Ok(resource);
    }

    /// <summary>
    /// Updates the status of a Work Order.
    /// </summary>
    /// <param name="workOrderId">The ID of the Work Order.</param>
    /// <param name="resource">The resource containing the new status (as string).</param>
    /// <returns>The updated Work Order resource.</returns>
    [HttpPatch("{workOrderId:int}/status")]
    [SwaggerOperation(
        Summary = "Update Work Order Status",
        Description = "Updates the status of a work order. This will also reflect the status in the associated Service Request.",
        OperationId = "UpdateWorkOrderStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "Work Order status updated successfully", typeof(WorkOrderResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid status or status transition")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Work Order not found")]
    public async Task<IActionResult> UpdateWorkOrderStatus(int workOrderId, [FromBody] UpdateWorkOrderStatusResource resource)
    {
        var command = UpdateWorkOrderStatusCommandFromResourceAssembler.ToCommandFromResource(workOrderId, resource);
        var workOrder = await workOrderCommandService.Handle(command);
        if (workOrder == null)
        {
            return NotFound();
        }
        var updatedResource = WorkOrderResourceFromEntityAssembler.ToResourceFromEntity(workOrder);
        return Ok(updatedResource);
    }
    
     /// <summary>
    /// Adds resolution details to a Work Order.
    /// </summary>
    /// <param name="workOrderId">The ID of the Work Order.</param>
    /// <param name="resource">The resource containing resolution details.</param>
    /// <returns>The updated Work Order resource.</returns>
    [HttpPatch("{workOrderId:int}/resolution")]
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

}


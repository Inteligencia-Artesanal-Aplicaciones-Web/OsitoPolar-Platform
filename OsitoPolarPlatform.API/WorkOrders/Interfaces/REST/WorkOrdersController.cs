using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Queries;
using OsitoPolarPlatform.API.WorkOrders.Domain.Services;
using OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Commands;
using OsitoPolarPlatform.API.IAM.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using OsitoPolarPlatform.API.Profiles.Interfaces.ACL;
using OsitoPolarPlatform.API.EquipmentManagement.Interfaces.ACL;

namespace OsitoPolarPlatform.API.WorkOrders.Interfaces.REST;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Work Order Endpoints")]
public class WorkOrdersController(
    IWorkOrderCommandService workOrderCommandService,
    IWorkOrderQueryService workOrderQueryService,
    IProfilesContextFacade profilesFacade,
    IEquipmentContextFacade equipmentFacade) : ControllerBase
{
    /// <summary>
    /// Helper method to get the authenticated owner ID
    /// </summary>
    /// <returns>The owner ID or error result</returns>
    private async Task<(ActionResult? error, int? ownerId)> GetAuthenticatedOwnerId()
    {
        var user = (User?)HttpContext.Items["User"];
        if (user == null)
            return (Unauthorized(new { message = "User not authenticated" }), null);

        var isOwner = await profilesFacade.IsUserAnOwner(user.Id);
        if (!isOwner)
            return (StatusCode(StatusCodes.Status403Forbidden,
                new { message = "User is not an owner. Only owners can manage work orders." }), null);

        var ownerId = await profilesFacade.FetchOwnerIdByUserId(user.Id);
        if (ownerId == 0)
            return (StatusCode(StatusCodes.Status403Forbidden,
                new { message = "Owner profile not found." }), null);

        return (null, ownerId);
    }

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
    /// Gets all Work Orders for the authenticated owner's equipment.
    /// </summary>
    /// <returns>A list of Work Order resources for the authenticated owner's equipment only.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get All Work Orders",
        Description = "Returns a list of all work orders for the authenticated owner's equipment only.",
        OperationId = "GetAllWorkOrders")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of work orders", typeof(IEnumerable<WorkOrderResource>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User not authenticated")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not an owner")]
    public async Task<IActionResult> GetAllWorkOrders()
    {
        // Get authenticated owner
        var (error, ownerId) = await GetAuthenticatedOwnerId();
        if (error != null) return error;

        // Get all equipment IDs owned by this owner
        var equipmentIds = (await equipmentFacade.FetchEquipmentIdsByOwnerId(ownerId!.Value)).ToHashSet();

        // Get all work orders and filter by owner's equipment
        var getAllWorkOrdersQuery = new GetAllWorkOrdersQuery();
        var allWorkOrders = await workOrderQueryService.Handle(getAllWorkOrdersQuery);
        var ownerWorkOrders = allWorkOrders.Where(wo => equipmentIds.Contains(wo.EquipmentId));

        var resources = ownerWorkOrders.Select(WorkOrderResourceFromEntityAssembler.ToResourceFromEntity).ToList();
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
        Description = "Returns a work order by its unique identifier if it belongs to the authenticated owner.",
        OperationId = "GetWorkOrderById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Work Order found", typeof(WorkOrderResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Work Order not found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Work order does not belong to the authenticated owner")]
    public async Task<IActionResult> GetWorkOrderById(int workOrderId)
    {
        // Get authenticated owner
        var (error, ownerId) = await GetAuthenticatedOwnerId();
        if (error != null) return error;

        var getWorkOrderByIdQuery = new GetWorkOrderByIdQuery(workOrderId);
        var workOrder = await workOrderQueryService.Handle(getWorkOrderByIdQuery);
        if (workOrder is null)
        {
            return NotFound();
        }

        // Verify ownership via equipment
        var isOwned = await equipmentFacade.IsEquipmentOwnedBy(workOrder.EquipmentId, ownerId!.Value);
        if (!isOwned)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You don't have permission to view this work order" });

        var resource = WorkOrderResourceFromEntityAssembler.ToResourceFromEntity(workOrder);
        return Ok(resource);
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
        Description = "Updates the status of a work order. This will also reflect the status in the associated Service Request. Requires ownership of the equipment.",
        OperationId = "UpdateWorkOrderStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "Work Order status updated successfully", typeof(WorkOrderResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid status or status transition")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Work Order not found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Work order does not belong to the authenticated owner")]
    public async Task<IActionResult> UpdateWorkOrderStatus(int workOrderId, [FromBody] UpdateWorkOrderStatusResource resource)
    {
        // Get authenticated owner
        var (error, ownerId) = await GetAuthenticatedOwnerId();
        if (error != null) return error;

        // Verify ownership
        var workOrder = await workOrderQueryService.Handle(new GetWorkOrderByIdQuery(workOrderId));
        if (workOrder == null)
            return NotFound();

        var isOwned = await equipmentFacade.IsEquipmentOwnedBy(workOrder.EquipmentId, ownerId!.Value);
        if (!isOwned)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You don't have permission to modify this work order" });

        var command = UpdateWorkOrderStatusCommandFromResourceAssembler.ToCommandFromResource(workOrderId, resource);
        var updatedWorkOrder = await workOrderCommandService.Handle(command);
        if (updatedWorkOrder == null)
        {
            return NotFound();
        }
        var updatedResource = WorkOrderResourceFromEntityAssembler.ToResourceFromEntity(updatedWorkOrder);
        return Ok(updatedResource);
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
        Description = "Adds resolution details and marks the work order as resolved. Requires ownership of the equipment.",
        OperationId = "AddWorkOrderResolutionDetails")]
    [SwaggerResponse(StatusCodes.Status200OK, "Resolution details added", typeof(WorkOrderResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Work Order not found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Work order does not belong to the authenticated owner")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input or failed to add resolution")]
    public async Task<IActionResult> AddWorkOrderResolutionDetails(int workOrderId, [FromBody] AddWorkOrderResolutionDetailsCommand resource)
    {
        // Get authenticated owner
        var (error, ownerId) = await GetAuthenticatedOwnerId();
        if (error != null) return error;

        // Verify ownership
        var workOrder = await workOrderQueryService.Handle(new GetWorkOrderByIdQuery(workOrderId));
        if (workOrder == null)
            return NotFound("Work Order not found.");

        var isOwned = await equipmentFacade.IsEquipmentOwnedBy(workOrder.EquipmentId, ownerId!.Value);
        if (!isOwned)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You don't have permission to modify this work order" });

        try
        {
            var command = new AddWorkOrderResolutionDetailsCommand(
                workOrderId,
                resource.ResolutionDetails,
                resource.TechnicianNotes,
                resource.Cost
            );
            var updatedWorkOrder = await workOrderCommandService.Handle(command);
            if (updatedWorkOrder == null) return NotFound("Work Order not found.");
            var workOrderResource = WorkOrderResourceFromEntityAssembler.ToResourceFromEntity(updatedWorkOrder);
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


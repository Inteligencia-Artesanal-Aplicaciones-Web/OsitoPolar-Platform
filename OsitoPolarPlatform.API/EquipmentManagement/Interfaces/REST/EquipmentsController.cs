using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Queries;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Services;
using OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Transform;

namespace OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Equipment Endpoints")]
public class EquipmentsController(
    IEquipmentCommandService equipmentCommandService,
    IEquipmentQueryService equipmentQueryService) : ControllerBase
{
    /// <summary>
    /// Gets equipment by its unique identifier.
    /// </summary>
    /// <param name="equipmentId">The unique identifier of the equipment to retrieve.</param>
    /// <returns>An EquipmentResource if found, or 404 Not Found if not found.</returns>
    [HttpGet("{equipmentId:int}")]
    [SwaggerOperation(
        Summary = "Get Equipment by Id",
        Description = "Returns equipment by its unique identifier.",
        OperationId = "GetEquipmentById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Equipment found", typeof(EquipmentResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    public async Task<IActionResult> GetEquipmentById(int equipmentId)
    {
        var getEquipmentByIdQuery = new GetEquipmentByIdQuery(equipmentId);
        var equipment = await equipmentQueryService.Handle(getEquipmentByIdQuery);
        if (equipment is null) return NotFound();
        var resource = EquipmentResourceFromEntityAssembler.ToResourceFromEntity(equipment);
        return Ok(resource);
    }

    /// <summary>
    /// Creates a new equipment in the system.
    /// </summary>
    /// <param name="resource">The resource containing the details of the equipment to create.</param>
    /// <returns>The created equipment resource with a 201 Created status code, or 400 Bad Request if creation failed.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create Equipment",
        Description = "Creates a new equipment in the system.",
        OperationId = "CreateEquipment")]
    [SwaggerResponse(StatusCodes.Status201Created, "Equipment created", typeof(EquipmentResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The equipment could not be created")]
    public async Task<IActionResult> CreateEquipment([FromBody] CreateEquipmentResource resource)
    {
        try
        {
            var createEquipmentCommand = CreateEquipmentCommandFromResourceAssembler.ToCommandFromResource(resource);
            var equipment = await equipmentCommandService.Handle(createEquipmentCommand);
            if (equipment is null) return BadRequest("Equipment could not be created.");
            var createdResource = EquipmentResourceFromEntityAssembler.ToResourceFromEntity(equipment);
            return CreatedAtAction(nameof(GetEquipmentById), new { equipmentId = createdResource.Id }, createdResource);
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
    /// Gets all equipment in the system.
    /// </summary>
    /// <returns>A list of all equipment as resources.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get All Equipment",
        Description = "Returns a list of all equipment in the system.",
        OperationId = "GetAllEquipments")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of equipment", typeof(IEnumerable<EquipmentResource>))]
    public async Task<IActionResult> GetAllEquipments()
    {
        var getAllEquipmentsQuery = new GetAllEquipmentsQuery();
        var equipments = await equipmentQueryService.Handle(getAllEquipmentsQuery);
        var resources = equipments.Select(EquipmentResourceFromEntityAssembler.ToResourceFromEntity).ToList();
        return Ok(resources);
    }

    /// <summary>
    /// Gets equipment by owner ID.
    /// </summary>
    /// <param name="ownerId">The ID of the owner.</param>
    /// <returns>A list of equipment owned by the specified owner.</returns>
    [HttpGet("owner/{ownerId:int}")]
    [SwaggerOperation(
        Summary = "Get Equipment by Owner",
        Description = "Returns equipment owned by a specific owner.",
        OperationId = "GetEquipmentsByOwnerId")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of equipment for the owner", typeof(IEnumerable<EquipmentResource>))]
    public async Task<IActionResult> GetEquipmentsByOwnerId(int ownerId)
    {
        var getEquipmentsByOwnerIdQuery = new GetEquipmentsByOwnerIdQuery(ownerId);
        var equipments = await equipmentQueryService.Handle(getEquipmentsByOwnerIdQuery);
        var resources = equipments.Select(EquipmentResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    /// <summary>
    /// Updates the temperature setting of equipment.
    /// </summary>
    /// <param name="equipmentId">The ID of the equipment.</param>
    /// <param name="resource">The resource containing the new temperature.</param>
    /// <returns>The updated equipment resource.</returns>
    [HttpPut("{equipmentId:int}/temperature")]
    [SwaggerOperation(
        Summary = "Update Equipment Temperature",
        Description = "Updates the temperature setting of equipment.",
        OperationId = "UpdateEquipmentTemperature")]
    [SwaggerResponse(StatusCodes.Status200OK, "Temperature updated", typeof(EquipmentResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid temperature or equipment is powered off")]
    public async Task<IActionResult> UpdateEquipmentTemperature(int equipmentId, [FromBody] UpdateEquipmentTemperatureResource resource)
    {
        try
        {
            var command = UpdateEquipmentTemperatureCommandFromResourceAssembler.ToCommandFromResource(equipmentId, resource);
            var equipment = await equipmentCommandService.Handle(command);
            if (equipment is null) return NotFound("Equipment not found.");
            var equipmentResource = EquipmentResourceFromEntityAssembler.ToResourceFromEntity(equipment);
            return Ok(equipmentResource);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates the power state of equipment.
    /// </summary>
    /// <param name="equipmentId">The ID of the equipment.</param>
    /// <param name="resource">The resource containing the new power state.</param>
    /// <returns>The updated equipment resource.</returns>
    [HttpPut("{equipmentId:int}/power")]
    [SwaggerOperation(
        Summary = "Update Equipment Power State",
        Description = "Updates the power state of equipment.",
        OperationId = "UpdateEquipmentPowerState")]
    [SwaggerResponse(StatusCodes.Status200OK, "Power state updated", typeof(EquipmentResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    public async Task<IActionResult> UpdateEquipmentPowerState(int equipmentId, [FromBody] UpdateEquipmentPowerStateResource resource)
    {
        var command = UpdateEquipmentPowerStateCommandFromResourceAssembler.ToCommandFromResource(equipmentId, resource);
        var equipment = await equipmentCommandService.Handle(command);
        if (equipment is null) return NotFound("Equipment not found.");
        var equipmentResource = EquipmentResourceFromEntityAssembler.ToResourceFromEntity(equipment);
        return Ok(equipmentResource);
    }

    /// <summary>
    /// Updates the location of equipment.
    /// </summary>
    /// <param name="equipmentId">The ID of the equipment.</param>
    /// <param name="resource">The resource containing the new location information.</param>
    /// <returns>The updated equipment resource.</returns>
    [HttpPut("{equipmentId:int}/location")]
    [SwaggerOperation(
        Summary = "Update Equipment Location",
        Description = "Updates the location information of equipment.",
        OperationId = "UpdateEquipmentLocation")]
    [SwaggerResponse(StatusCodes.Status200OK, "Location updated", typeof(EquipmentResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid location data")]
    public async Task<IActionResult> UpdateEquipmentLocation(int equipmentId, [FromBody] UpdateEquipmentLocationResource resource)
    {
        try
        {
            var command = UpdateEquipmentLocationCommandFromResourceAssembler.ToCommandFromResource(equipmentId, resource);
            var equipment = await equipmentCommandService.Handle(command);
            if (equipment is null) return NotFound("Equipment not found.");
            var equipmentResource = EquipmentResourceFromEntityAssembler.ToResourceFromEntity(equipment);
            return Ok(equipmentResource);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
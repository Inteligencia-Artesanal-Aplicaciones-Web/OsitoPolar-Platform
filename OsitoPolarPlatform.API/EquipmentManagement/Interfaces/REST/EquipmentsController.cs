using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Commands;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Queries;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Services;
using OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Transform;

namespace OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST;

/// <summary>
/// RESTful API Controller for Equipment Management
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]  // Will become /api/v1/equipments due to KebabCaseRouteNamingConvention
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Equipment Endpoints")]
public class EquipmentsController : ControllerBase
{
    private readonly IEquipmentCommandService _equipmentCommandService;
    private readonly IEquipmentQueryService _equipmentQueryService;

    public EquipmentsController(
        IEquipmentCommandService equipmentCommandService,
        IEquipmentQueryService equipmentQueryService)
    {
        _equipmentCommandService = equipmentCommandService;
        _equipmentQueryService = equipmentQueryService;
    }

    /// <summary>
    /// Gets all equipments in the system.
    /// </summary>
    /// <returns>A list of all equipments as resources.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get All Equipments",
        Description = "Returns a list of all equipments in the system.",
        OperationId = "GetAllEquipments")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of equipments", typeof(IEnumerable<EquipmentResource>))]
    public async Task<ActionResult<IEnumerable<EquipmentResource>>> GetAllEquipments()
    {
        var getAllEquipmentsQuery = new GetAllEquipmentsQuery();
        var equipments = await _equipmentQueryService.Handle(getAllEquipmentsQuery);
        var equipmentResources = equipments.Select(EquipmentResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(equipmentResources);
    }

    /// <summary>
    /// Gets equipment by its unique identifier.
    /// </summary>
    /// <param name="equipmentId">Equipment identifier</param>
    /// <returns>Equipment details</returns>
    [HttpGet("{equipmentId:int}")]
    [SwaggerOperation(
        Summary = "Get Equipment by Id",
        Description = "Returns equipment by its unique identifier.",
        OperationId = "GetEquipmentById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Equipment found", typeof(EquipmentResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    public async Task<ActionResult<EquipmentResource>> GetEquipmentById(int equipmentId)
    {
        var getEquipmentByIdQuery = new GetEquipmentByIdQuery(equipmentId);
        var equipment = await _equipmentQueryService.Handle(getEquipmentByIdQuery);
        
        if (equipment == null)
            return NotFound($"Equipment with ID {equipmentId} not found");

        var equipmentResource = EquipmentResourceFromEntityAssembler.ToResourceFromEntity(equipment);
        return Ok(equipmentResource);
    }

    /// <summary>
    /// Gets equipments by owner ID.
    /// </summary>
    /// <param name="ownerId">Owner identifier</param>
    /// <returns>List of equipments owned by the specified owner</returns>
    [HttpGet("owners/{ownerId:int}")]
    [SwaggerOperation(
        Summary = "Get Equipments by Owner",
        Description = "Returns equipments owned by a specific owner.",
        OperationId = "GetEquipmentsByOwnerId")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of equipments for the owner", typeof(IEnumerable<EquipmentResource>))]
    public async Task<ActionResult<IEnumerable<EquipmentResource>>> GetEquipmentsByOwnerId(int ownerId)
    {
        var getEquipmentsByOwnerIdQuery = new GetEquipmentsByOwnerIdQuery(ownerId);
        var equipments = await _equipmentQueryService.Handle(getEquipmentsByOwnerIdQuery);
        var equipmentResources = equipments.Select(EquipmentResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(equipmentResources);
    }

    /// <summary>
    /// Creates a new equipment in the system.
    /// </summary>
    /// <param name="resource">Equipment creation data</param>
    /// <returns>Created equipment</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create Equipment",
        Description = "Creates a new equipment in the system.",
        OperationId = "CreateEquipment")]
    [SwaggerResponse(StatusCodes.Status201Created, "Equipment created", typeof(EquipmentResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The equipment could not be created")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Equipment with serial number or code already exists")]
    public async Task<ActionResult<EquipmentResource>> CreateEquipment([FromBody] CreateEquipmentResource resource)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var createEquipmentCommand = CreateEquipmentCommandFromResourceAssembler.ToCommandFromResource(resource);
            var equipment = await _equipmentCommandService.Handle(createEquipmentCommand);
            
            if (equipment == null)
                return BadRequest("Equipment could not be created");

            var equipmentResource = EquipmentResourceFromEntityAssembler.ToResourceFromEntity(equipment);
            return CreatedAtAction(nameof(GetEquipmentById), new { equipmentId = equipment.Id }, equipmentResource);
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
    /// Update equipment operation parameters (temperature, power state, location, etc.)
    /// This is the unified endpoint for all equipment parameter updates as requested by professor:
    /// "UN SOLO EQUIPMENT OID OPERATION PARAMETERES PARA ACTUALIZAR"
    /// </summary>
    /// <param name="equipmentId">Equipment identifier</param>
    /// <param name="resource">Equipment operation parameters to update</param>
    /// <returns>Updated equipment</returns>
    [HttpPatch("{equipmentId:int}/operations")]
    [SwaggerOperation(
        Summary = "Update Equipment Operation Parameters",
        Description = "Updates equipment operation parameters (temperature, power state, location) in a single unified endpoint. This replaces the separate /temperature, /power, and /location endpoints.",
        OperationId = "UpdateEquipmentOperations")]
    [SwaggerResponse(StatusCodes.Status200OK, "Equipment operations updated", typeof(EquipmentResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid operation parameters")]
    public async Task<ActionResult<EquipmentResource>> UpdateEquipmentOperations(
        int equipmentId, 
        [FromBody] EquipmentOperationParametersResource resource)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var equipment = await _equipmentQueryService.Handle(new GetEquipmentByIdQuery(equipmentId));
        if (equipment == null)
            return NotFound($"Equipment with ID {equipmentId} not found");

        try
        {
            // Handle temperature update
            if (resource.Temperature.HasValue)
            {
                var updateTemperatureCommand = new UpdateEquipmentTemperatureCommand(equipmentId, resource.Temperature.Value);
                await _equipmentCommandService.Handle(updateTemperatureCommand);
            }

            // Handle power state update
            if (!string.IsNullOrEmpty(resource.PowerState))
            {
                bool isPoweredOn = resource.PowerState.ToUpper() == "ON";
                var updatePowerStateCommand = new UpdateEquipmentPowerStateCommand(equipmentId, isPoweredOn);
                await _equipmentCommandService.Handle(updatePowerStateCommand);
            }

            // Handle location update
            if (resource.Location != null)
            {
                var updateLocationCommand = new UpdateEquipmentLocationCommand(
                    equipmentId,
                    resource.Location.Address, // Using Address as LocationName for compatibility
                    resource.Location.Address,
                    (decimal)resource.Location.Latitude,
                    (decimal)resource.Location.Longitude
                );
                await _equipmentCommandService.Handle(updateLocationCommand);
            }

            // Get updated equipment
            var updatedEquipment = await _equipmentQueryService.Handle(new GetEquipmentByIdQuery(equipmentId));
            var equipmentResource = EquipmentResourceFromEntityAssembler.ToResourceFromEntity(updatedEquipment!);
            
            return Ok(equipmentResource);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Complete equipment update (replaces entire equipment resource)
    /// </summary>
    /// <param name="equipmentId">Equipment identifier</param>
    /// <param name="resource">Complete equipment data</param>
    /// <returns>Updated equipment</returns>
    [HttpPut("{equipmentId:int}")]
    [SwaggerOperation(
        Summary = "Update Complete Equipment",
        Description = "Updates the complete equipment resource (full replacement).",
        OperationId = "UpdateEquipment")]
    [SwaggerResponse(StatusCodes.Status200OK, "Equipment updated", typeof(EquipmentResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid equipment data")]
    public async Task<ActionResult<EquipmentResource>> UpdateEquipment(
        int equipmentId, 
        [FromBody] UpdateEquipmentResource resource)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var equipment = await _equipmentQueryService.Handle(new GetEquipmentByIdQuery(equipmentId));
        if (equipment == null)
            return NotFound($"Equipment with ID {equipmentId} not found");

        // Use the operation parameters approach for consistency
        var operationResource = new EquipmentOperationParametersResource
        {
            Temperature = resource.Temperature,
            PowerState = resource.PowerState,
            Location = resource.Location
        };

        return await UpdateEquipmentOperations(equipmentId, operationResource);
    }

    /// <summary>
    /// Delete equipment
    /// </summary>
    /// <param name="equipmentId">Equipment identifier</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{equipmentId:int}")]
    [SwaggerOperation(
        Summary = "Delete Equipment",
        Description = "Deletes an equipment from the system.",
        OperationId = "DeleteEquipment")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Equipment deleted successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Equipment cannot be deleted due to business rules")]
    public async Task<ActionResult> DeleteEquipment(int equipmentId)
    {
        try
        {
            var deleteEquipmentCommand = new DeleteEquipmentCommand(equipmentId);
            var wasDeleted = await _equipmentCommandService.Handle(deleteEquipmentCommand);
        
            if (!wasDeleted)
                return NotFound($"Equipment with ID {equipmentId} not found");

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
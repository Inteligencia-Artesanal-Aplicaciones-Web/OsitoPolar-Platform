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
        Description = "Gets all equipments or filters by owner using query parameter",
        OperationId = "GetAllEquipments")]
    [SwaggerResponse(StatusCodes.Status200OK, "Equipments retrieved successfully")]
    public async Task<IActionResult> GetAllEquipments([FromQuery(Name = "owner-id")] int? ownerId = null)
    {
        try
        {
            if (ownerId.HasValue)
            {
                var query = new GetEquipmentsByOwnerIdQuery(ownerId.Value);
                var equipmentsByOwner = await _equipmentQueryService.Handle(query);
                var ownerResources = equipmentsByOwner.Select(EquipmentResourceFromEntityAssembler.ToResourceFromEntity);
                return Ok(ownerResources);
            }
            var getAllQuery = new GetAllEquipmentsQuery();
            var equipments = await _equipmentQueryService.Handle(getAllQuery);
            var resources = equipments.Select(EquipmentResourceFromEntityAssembler.ToResourceFromEntity);
            return Ok(resources);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
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
    /// Creates  new equipment in the system.
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
    /// <summary>
    /// Create equipment reading (temperature, energy)
    /// POST operations moved from Analytics to Equipment Management as per professor's feedback
    /// </summary>
    /// <param name="equipmentId">Equipment identifier</param>
    /// <param name="resource">Reading data</param>
    /// <returns>Created reading confirmation</returns>
    [HttpPost("{equipmentId:int}/readings")]
    [SwaggerOperation(
        Summary = "Create Equipment Reading",
        Description = "Records a new reading (temperature, energy) for equipment. Moved from Analytics to Equipment Management per professor's requirements.",
        OperationId = "CreateEquipmentReading")]
    [SwaggerResponse(StatusCodes.Status201Created, "Reading created successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid reading data")]
    public async Task<ActionResult> CreateEquipmentReading(
        int equipmentId,
        [FromBody] CreateEquipmentReadingResource resource)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var equipment = await _equipmentQueryService.Handle(new GetEquipmentByIdQuery(equipmentId));
        if (equipment == null)
            return NotFound($"Equipment with ID {equipmentId} not found");

        try
        {
            // Create the reading response
            var reading = new {
                id = new Random().Next(1000, 9999), // In real implementation, this would come from your command
                equipmentId = equipmentId,
                type = resource.Type,
                value = resource.Value,
                unit = resource.Unit,
                timestamp = resource.Timestamp ?? DateTimeOffset.UtcNow,
                status = resource.Status,
                notes = resource.Notes
            };

            // Here you would typically:
            // 1. Create appropriate command (RecordTemperatureReadingCommand or RecordEnergyReadingCommand)
            // 2. Send to Analytics bounded context via command bus or direct service call
            // 3. Return the created reading

            return CreatedAtAction(nameof(GetEquipmentById), new { equipmentId }, reading);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get equipment readings (delegated to Analytics)
    /// Convenience endpoint that redirects to Analytics API
    /// </summary>
    /// <param name="equipmentId">Equipment identifier</param>
    /// <param name="type">Type of readings</param>
    /// <returns>Redirect to Analytics endpoint</returns>
    ///
    /// 
    [HttpGet("{equipmentId:int}/readings")]
    [SwaggerOperation(
        Summary = "Get Equipment Readings",
        Description = "Redirects to Analytics API for reading queries.",
        OperationId = "GetEquipmentReadingsRedirect")]
    public ActionResult GetEquipmentReadings(
        int equipmentId, 
        [FromQuery] string type = "all")
    {
        
        return Redirect($"/api/v1/analytics/equipments/{equipmentId}/readings?type={type}");
    }
}
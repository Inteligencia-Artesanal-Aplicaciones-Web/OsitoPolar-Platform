using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using OsitoPolarPlatform.API.Analytics.Domain.Model.Queries;
using OsitoPolarPlatform.API.Analytics.Domain.Services;
using OsitoPolarPlatform.API.Analytics.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.Analytics.Interfaces.REST.Transform;

namespace OsitoPolarPlatform.API.Analytics.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Analytics Endpoints")]
public class AnalyticsController(
    IAnalyticsCommandService analyticsCommandService,
    IAnalyticsQueryService analyticsQueryService) : ControllerBase
{
    /// <summary>
    /// Gets temperature readings for equipment
    /// </summary>
    /// <param name="equipmentId">Equipment ID</param>
    /// <param name="hours">Hours to look back (default 24)</param>
    /// <returns>List of temperature readings</returns>
    [HttpGet("equipment/{equipmentId:int}/temperature-readings")]
    [SwaggerOperation(
        Summary = "Get Temperature Readings",
        Description = "Returns temperature readings for the specified equipment",
        OperationId = "GetTemperatureReadings")]
    [SwaggerResponse(StatusCodes.Status200OK, "Temperature readings found", typeof(IEnumerable<TemperatureReadingResource>))]
    public async Task<IActionResult> GetTemperatureReadings(int equipmentId, [FromQuery] int hours = 24)
    {
        var query = new GetTemperatureReadingsQuery(equipmentId, hours);
        var readings = await analyticsQueryService.Handle(query);
        var resources = readings.Select(TemperatureReadingResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    /// <summary>
    /// Gets daily temperature averages for equipment
    /// </summary>
    /// <param name="equipmentId">Equipment ID</param>
    /// <param name="days">Days to look back (default 7)</param>
    /// <returns>List of daily temperature averages</returns>
    [HttpGet("equipment/{equipmentId:int}/daily-temperature-averages")]
    [SwaggerOperation(
        Summary = "Get Daily Temperature Averages",
        Description = "Returns daily temperature averages for the specified equipment",
        OperationId = "GetDailyTemperatureAverages")]
    [SwaggerResponse(StatusCodes.Status200OK, "Daily averages found", typeof(IEnumerable<DailyTemperatureAverageResource>))]
    public async Task<IActionResult> GetDailyTemperatureAverages(int equipmentId, [FromQuery] int days = 7)
    {
        var query = new GetDailyTemperatureAveragesQuery(equipmentId, days);
        var averages = await analyticsQueryService.Handle(query);
        var resources = averages.Select(DailyTemperatureAverageResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    /// <summary>
    /// Gets energy readings for equipment
    /// </summary>
    /// <param name="equipmentId">Equipment ID</param>
    /// <param name="hours">Hours to look back (default 24)</param>
    /// <returns>List of energy readings</returns>
    [HttpGet("equipment/{equipmentId:int}/energy-readings")]
    [SwaggerOperation(
        Summary = "Get Energy Readings",
        Description = "Returns energy consumption readings for the specified equipment",
        OperationId = "GetEnergyReadings")]
    [SwaggerResponse(StatusCodes.Status200OK, "Energy readings found", typeof(IEnumerable<EnergyReadingResource>))]
    public async Task<IActionResult> GetEnergyReadings(int equipmentId, [FromQuery] int hours = 24)
    {
        var query = new GetEnergyReadingsQuery(equipmentId, hours);
        var readings = await analyticsQueryService.Handle(query);
        var resources = readings.Select(EnergyReadingResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    /// <summary>
    /// Records a new temperature reading
    /// </summary>
    /// <param name="resource">Temperature reading data</param>
    /// <returns>Created temperature reading</returns>
    [HttpPost("temperature-readings")]
    [SwaggerOperation(
        Summary = "Record Temperature Reading",
        Description = "Records a new temperature reading for equipment",
        OperationId = "RecordTemperatureReading")]
    [SwaggerResponse(StatusCodes.Status201Created, "Temperature reading recorded", typeof(TemperatureReadingResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid temperature reading data")]
    public async Task<IActionResult> RecordTemperatureReading([FromBody] CreateTemperatureReadingResource resource)
    {
        try
        {
            var command = CreateTemperatureReadingCommandFromResourceAssembler.ToCommandFromResource(resource);
            var reading = await analyticsCommandService.Handle(command);
            if (reading is null) return BadRequest("Failed to record temperature reading.");
            var createdResource = TemperatureReadingResourceFromEntityAssembler.ToResourceFromEntity(reading);
            return Created($"/api/v1/analytics/temperature-readings/{createdResource.Id}", createdResource);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Records a new energy reading
    /// </summary>
    /// <param name="resource">Energy reading data</param>
    /// <returns>Created energy reading</returns>
    [HttpPost("energy-readings")]
    [SwaggerOperation(
        Summary = "Record Energy Reading",
        Description = "Records a new energy consumption reading for equipment",
        OperationId = "RecordEnergyReading")]
    [SwaggerResponse(StatusCodes.Status201Created, "Energy reading recorded", typeof(EnergyReadingResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid energy reading data")]
    public async Task<IActionResult> RecordEnergyReading([FromBody] CreateEnergyReadingResource resource)
    {
        try
        {
            var command = CreateEnergyReadingCommandFromResourceAssembler.ToCommandFromResource(resource);
            var reading = await analyticsCommandService.Handle(command);
            if (reading is null) return BadRequest("Failed to record energy reading.");
            var createdResource = EnergyReadingResourceFromEntityAssembler.ToResourceFromEntity(reading);
            return Created($"/api/v1/analytics/energy-readings/{createdResource.Id}", createdResource);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
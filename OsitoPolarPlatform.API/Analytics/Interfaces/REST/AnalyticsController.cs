using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using OsitoPolarPlatform.API.Analytics.Domain.Model.Queries;
using OsitoPolarPlatform.API.Analytics.Domain.Services;
using OsitoPolarPlatform.API.Analytics.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.Analytics.Interfaces.REST.Transform;

namespace OsitoPolarPlatform.API.Analytics.Interfaces.REST;

/// <summary>
/// RESTful API Controller for Equipment Analytics 
/// Analytics = Queries only, Commands moved to Equipment Management
/// </summary>
[ApiController]
[Route("api/v1/analytics/equipments")]  // Plural route for consistency
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Analytics Endpoints")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsQueryService _analyticsQueryService;

    public AnalyticsController(IAnalyticsQueryService analyticsQueryService)
    {
        _analyticsQueryService = analyticsQueryService;
    }

    /// <summary>
    /// Get equipment readings (temperature, energy, all)
    /// Unified endpoint that replaces separate temperature-readings and energy-readings
    /// </summary>
    /// <param name="equipmentId">Equipment identifier</param>
    /// <param name="type">Type of readings: temperature, energy, all</param>
    /// <param name="hours">Hours to look back (default 24)</param>
    /// <param name="limit">Maximum number of readings (default 100)</param>
    /// <returns>Unified list of readings</returns>
    [HttpGet("{equipmentId:int}/readings")]
    [SwaggerOperation(
        Summary = "Get Equipment Readings", 
        Description = "Retrieves equipment readings (temperature, energy) with flexible filtering. Replaces separate /temperature-readings and /energy-readings endpoints.",
        OperationId = "GetEquipmentReadings")]
    [SwaggerResponse(StatusCodes.Status200OK, "Readings retrieved successfully", typeof(UnifiedReadingResponse))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    public async Task<ActionResult<UnifiedReadingResponse>> GetEquipmentReadings(
        int equipmentId,
        [FromQuery] string type = "all",        // temperature, energy, all
        [FromQuery] int hours = 24,
        [FromQuery] int limit = 100)
    {
        try
        {
            var readings = new List<UnifiedReadingResource>();

            // Get temperature readings
            if (type == "temperature" || type == "all")
            {
                var tempQuery = new GetTemperatureReadingsQuery(equipmentId, hours);
                var tempReadings = await _analyticsQueryService.Handle(tempQuery);
                
                readings.AddRange(tempReadings.Take(limit).Select(reading => new UnifiedReadingResource
                {
                    Id = reading.Id,
                    EquipmentId = reading.EquipmentId,
                    Type = "temperature",
                    Value = reading.Temperature,
                    Unit = "celsius",
                    Timestamp = reading.Timestamp,
                    Status = reading.Status.ToString().ToLower()
                }));
            }

            // Get energy readings  
            if (type == "energy" || type == "all")
            {
                var energyQuery = new GetEnergyReadingsQuery(equipmentId, hours);
                var energyReadings = await _analyticsQueryService.Handle(energyQuery);
                
                readings.AddRange(energyReadings.Take(limit).Select(reading => new UnifiedReadingResource
                {
                    Id = reading.Id,
                    EquipmentId = reading.EquipmentId,
                    Type = "energy",
                    Value = reading.Consumption,
                    Unit = reading.Unit,
                    Timestamp = reading.Timestamp,
                    Status = reading.Status.ToString().ToLower()
                }));
            }

            var response = new UnifiedReadingResponse
            {
                Data = readings.OrderByDescending(r => r.Timestamp).Take(limit).ToList(),
                Total = readings.Count,
                EquipmentId = equipmentId,
                Type = type,
                Period = $"{hours}h"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get equipment analytics summaries (daily averages, trends, etc.)
    /// Unified endpoint that replaces daily-temperature-averages
    /// </summary>
    /// <param name="equipmentId">Equipment identifier</param>
    /// <param name="type">Summary type: daily-averages, weekly-trends</param>
    /// <param name="days">Days to look back (default 7)</param>
    /// <returns>Analytics summaries</returns>
    [HttpGet("{equipmentId:int}/summaries")]
    [SwaggerOperation(
        Summary = "Get Equipment Analytics Summaries",
        Description = "Retrieves processed analytics data like daily averages and trends. Replaces /daily-temperature-averages endpoint.",
        OperationId = "GetEquipmentSummaries")]
    [SwaggerResponse(StatusCodes.Status200OK, "Summaries retrieved successfully", typeof(AnalyticsSummaryResponse))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    public async Task<ActionResult<AnalyticsSummaryResponse>> GetEquipmentSummaries(
        int equipmentId,
        [FromQuery] string type = "daily-averages",  // daily-averages, weekly-trends
        [FromQuery] int days = 7)
    {
        try
        {
            var summaries = new List<AnalyticsSummaryResource>();

            if (type == "daily-averages")
            {
                var query = new GetDailyTemperatureAveragesQuery(equipmentId, days);
                var dailyAverages = await _analyticsQueryService.Handle(query);
                
                summaries.AddRange(dailyAverages.Select(avg => new AnalyticsSummaryResource
                {
                    Id = avg.Id,
                    EquipmentId = avg.EquipmentId,
                    Date = avg.Date.ToString("yyyy-MM-dd"),
                    Type = "daily-average",
                    AverageTemperature = avg.AverageTemperature,
                    MinTemperature = avg.MinTemperature,
                    MaxTemperature = avg.MaxTemperature
                }));
            }

            var response = new AnalyticsSummaryResponse
            {
                Data = summaries,
                Total = summaries.Count,
                EquipmentId = equipmentId,
                Type = type,
                Days = days
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get analytics overview for multiple equipments
    /// New efficient endpoint for dashboard views
    /// </summary>
    /// <param name="ids">Comma-separated equipment IDs</param>
    /// <param name="type">Data type: current, summary</param>
    /// <returns>Multi-equipment analytics overview</returns>
    [HttpGet("overview")]
    [SwaggerOperation(
        Summary = "Get Multiple Equipments Analytics Overview",
        Description = "Retrieves analytics overview for multiple equipments. New endpoint for efficient dashboard queries.",
        OperationId = "GetEquipmentsAnalyticsOverview")]
    [SwaggerResponse(StatusCodes.Status200OK, "Overview retrieved successfully")]
    public async Task<ActionResult> GetEquipmentsAnalyticsOverview(
        [FromQuery] string ids = "",
        [FromQuery] string type = "current")
    {
        try
        {
            var equipmentIds = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(int.Parse)
                                 .ToList();

            if (!equipmentIds.Any())
                return BadRequest("At least one equipment ID is required");
            
            var overview = new {
                equipments = equipmentIds.Select(id => new {
                    equipmentId = id,
                    lastTemperature = 0m, // Would come from latest reading
                    lastEnergyReading = 0m, // Would come from latest reading  
                    status = "normal",
                    lastReadingTime = DateTimeOffset.UtcNow
                }),
                summary = new {
                    totalEquipments = equipmentIds.Count,
                    normalCount = equipmentIds.Count,
                    warningCount = 0,
                    criticalCount = 0
                }
            };

            return Ok(overview);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using OsitoPolarPlatform.API.Analytics.Domain.Model.Queries;
using OsitoPolarPlatform.API.Analytics.Domain.Services;
using OsitoPolarPlatform.API.Analytics.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.Analytics.Interfaces.REST.Transform;
using OsitoPolarPlatform.API.IAM.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using OsitoPolarPlatform.API.Profiles.Domain.Repositories;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Repositories;

namespace OsitoPolarPlatform.API.Analytics.Interfaces.REST;

/// <summary>
/// RESTful API Controller for Equipment Analytics
/// Analytics = Queries only, Commands moved to Equipment Management
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/analytics/equipments")]  // Plural route for consistency
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Analytics Endpoints")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsQueryService _analyticsQueryService;
    private readonly IOwnerRepository _ownerRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    public AnalyticsController(
        IAnalyticsQueryService analyticsQueryService,
        IOwnerRepository ownerRepository,
        IEquipmentRepository equipmentRepository)
    {
        _analyticsQueryService = analyticsQueryService;
        _ownerRepository = ownerRepository;
        _equipmentRepository = equipmentRepository;
    }

    /// <summary>
    /// Helper method to get the authenticated owner profile
    /// </summary>
    /// <returns>The owner profile or error result</returns>
    private async Task<(ActionResult? error, Profiles.Domain.Model.Aggregates.Owner? owner)> GetAuthenticatedOwner()
    {
        var user = (User?)HttpContext.Items["User"];
        if (user == null)
            return (Unauthorized(new { message = "User not authenticated" }), null);

        var ownerProfile = await _ownerRepository.FindByUserIdAsync(user.Id);
        if (ownerProfile == null)
            return (StatusCode(StatusCodes.Status403Forbidden,
                new { message = "User is not an owner. Only owners can view analytics." }), null);

        return (null, ownerProfile);
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
        Description = "Retrieves equipment readings (temperature, energy) with flexible filtering. Only returns data for equipment owned by the authenticated user.",
        OperationId = "GetEquipmentReadings")]
    [SwaggerResponse(StatusCodes.Status200OK, "Readings retrieved successfully", typeof(UnifiedReadingResponse))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Equipment does not belong to the authenticated owner")]
    public async Task<ActionResult<UnifiedReadingResponse>> GetEquipmentReadings(
        int equipmentId,
        [FromQuery] string type = "all",        // temperature, energy, all
        [FromQuery] int hours = 24,
        [FromQuery] int limit = 100)
    {
        // Get authenticated owner
        var (error, ownerProfile) = await GetAuthenticatedOwner();
        if (error != null) return error;

        // Verify equipment ownership
        var equipment = await _equipmentRepository.FindByIdAsync(equipmentId);
        if (equipment == null)
            return NotFound(new { message = "Equipment not found" });

        if (equipment.OwnerId != ownerProfile!.Id)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You don't have permission to view analytics for this equipment" });

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
        Description = "Retrieves processed analytics data like daily averages and trends. Only returns data for equipment owned by the authenticated user.",
        OperationId = "GetEquipmentSummaries")]
    [SwaggerResponse(StatusCodes.Status200OK, "Summaries retrieved successfully", typeof(AnalyticsSummaryResponse))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Equipment does not belong to the authenticated owner")]
    public async Task<ActionResult<AnalyticsSummaryResponse>> GetEquipmentSummaries(
        int equipmentId,
        [FromQuery] string type = "daily-averages",  // daily-averages, weekly-trends
        [FromQuery] int days = 7)
    {
        // Get authenticated owner
        var (error, ownerProfile) = await GetAuthenticatedOwner();
        if (error != null) return error;

        // Verify equipment ownership
        var equipment = await _equipmentRepository.FindByIdAsync(equipmentId);
        if (equipment == null)
            return NotFound(new { message = "Equipment not found" });

        if (equipment.OwnerId != ownerProfile!.Id)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You don't have permission to view analytics for this equipment" });

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
        Description = "Retrieves analytics overview for multiple equipments owned by the authenticated user. Equipment IDs are optional - if not provided, returns overview for all owner's equipment.",
        OperationId = "GetEquipmentsAnalyticsOverview")]
    [SwaggerResponse(StatusCodes.Status200OK, "Overview retrieved successfully")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Some equipment does not belong to the authenticated owner")]
    public async Task<ActionResult> GetEquipmentsAnalyticsOverview(
        [FromQuery] string ids = "",
        [FromQuery] string type = "current")
    {
        // Get authenticated owner
        var (error, ownerProfile) = await GetAuthenticatedOwner();
        if (error != null) return error;

        try
        {
            // Get owner's equipment
            var ownerEquipment = await _equipmentRepository.FindByOwnerIdAsync(ownerProfile!.Id);
            var ownerEquipmentIds = ownerEquipment.Select(e => e.Id).ToHashSet();

            List<int> equipmentIds;
            if (string.IsNullOrWhiteSpace(ids))
            {
                // If no IDs provided, use all owner's equipment
                equipmentIds = ownerEquipmentIds.ToList();
            }
            else
            {
                // Parse provided IDs
                equipmentIds = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(int.Parse)
                                 .ToList();

                // Verify all requested equipment belongs to the owner
                var unauthorizedIds = equipmentIds.Where(id => !ownerEquipmentIds.Contains(id)).ToList();
                if (unauthorizedIds.Any())
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new { message = $"You don't have permission to view analytics for equipment IDs: {string.Join(", ", unauthorizedIds)}" });
            }

            if (!equipmentIds.Any())
                return Ok(new { message = "No equipment found for this owner", equipments = new List<object>() });
            
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
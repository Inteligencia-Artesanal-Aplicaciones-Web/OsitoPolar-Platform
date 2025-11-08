using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using OsitoPolarPlatform.API.Analytics.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Analytics.Domain.Repositories;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Repositories;
using OsitoPolarPlatform.API.Profiles.Domain.Repositories;
using OsitoPolarPlatform.API.IAM.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;

namespace OsitoPolarPlatform.API.Analytics.Interfaces.REST;

/// <summary>
/// Advanced Analytics Controller
/// Provides health scores, anomaly detection, cost analysis, and maintenance forecasts
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/analytics/equipments")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Advanced Analytics - Health, Anomalies, Costs, Maintenance")]
public class AdvancedAnalyticsController : ControllerBase
{
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IOwnerRepository _ownerRepository;

    public AdvancedAnalyticsController(
        IAnalyticsRepository analyticsRepository,
        IEquipmentRepository equipmentRepository,
        IOwnerRepository ownerRepository)
    {
        _analyticsRepository = analyticsRepository;
        _equipmentRepository = equipmentRepository;
        _ownerRepository = ownerRepository;
    }

    /// <summary>
    /// Helper method to verify equipment ownership
    /// </summary>
    private async Task<(ActionResult? error, int ownerId)> VerifyOwnership(int equipmentId)
    {
        var user = (User?)HttpContext.Items["User"];
        if (user == null)
            return (Unauthorized(new { message = "User not authenticated" }), 0);

        var ownerProfile = await _ownerRepository.FindByUserIdAsync(user.Id);
        if (ownerProfile == null)
            return (StatusCode(StatusCodes.Status403Forbidden,
                new { message = "Only owners can view analytics" }), 0);

        var equipment = await _equipmentRepository.FindByIdAsync(equipmentId);
        if (equipment == null)
            return (NotFound(new { message = "Equipment not found" }), 0);

        if (equipment.OwnerId != ownerProfile.Id)
            return (StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You don't have permission to view this equipment's analytics" }), 0);

        return (null, ownerProfile.Id);
    }

    /// <summary>
    /// Get equipment health metrics
    /// </summary>
    [HttpGet("{equipmentId:int}/health")]
    [SwaggerOperation(
        Summary = "Get Equipment Health Score",
        Description = "Analyzes temperature stability and returns health metrics including variance, trends, and overall score",
        OperationId = "GetEquipmentHealth")]
    [SwaggerResponse(StatusCodes.Status200OK, "Health metrics calculated")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied")]
    public async Task<IActionResult> GetEquipmentHealth(
        int equipmentId,
        [FromQuery] int days = 7)
    {
        var (error, _) = await VerifyOwnership(equipmentId);
        if (error != null) return error;

        try
        {
            var cutoff = DateTimeOffset.UtcNow.AddDays(-days);
            var readings = await _analyticsRepository.FindTemperatureReadingsByDateRangeAsync(
                equipmentId,
                cutoff,
                DateTimeOffset.UtcNow
            );

            var readingsList = readings.ToList();
            var health = EquipmentAnalytics.CalculateHealth(readingsList);

            return Ok(health);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AdvancedAnalytics] Error calculating health: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Detect temperature anomalies
    /// </summary>
    [HttpGet("{equipmentId:int}/anomalies")]
    [SwaggerOperation(
        Summary = "Detect Temperature Anomalies",
        Description = "Detects unusual temperature patterns like door open, compressor failure, or power outage",
        OperationId = "DetectAnomalies")]
    [SwaggerResponse(StatusCodes.Status200OK, "Anomaly detection complete")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied")]
    public async Task<IActionResult> DetectAnomalies(
        int equipmentId,
        [FromQuery] int hours = 24)
    {
        var (error, _) = await VerifyOwnership(equipmentId);
        if (error != null) return error;

        try
        {
            var equipment = await _equipmentRepository.FindByIdAsync(equipmentId);
            var readings = await _analyticsRepository.FindTemperatureReadingsByEquipmentIdAsync(equipmentId, hours);

            var readingsList = readings.ToList();
            var anomaly = EquipmentAnalytics.DetectAnomalies(
                readingsList,
                equipment!.OptimalTemperatureMin,
                equipment.OptimalTemperatureMax
            );

            return Ok(anomaly);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AdvancedAnalytics] Error detecting anomalies: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get cost analysis
    /// </summary>
    [HttpGet("{equipmentId:int}/costs")]
    [SwaggerOperation(
        Summary = "Get Energy Cost Analysis",
        Description = "Calculates energy costs, compares to previous period, and provides cost-saving recommendations",
        OperationId = "GetCostAnalysis")]
    [SwaggerResponse(StatusCodes.Status200OK, "Cost analysis complete")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied")]
    public async Task<IActionResult> GetCostAnalysis(
        int equipmentId,
        [FromQuery] int days = 30,
        [FromQuery] decimal rate = 0.12m)
    {
        var (error, _) = await VerifyOwnership(equipmentId);
        if (error != null) return error;

        try
        {
            // Current period
            var currentStart = DateTimeOffset.UtcNow.AddDays(-days);
            var currentReadings = await _analyticsRepository.FindEnergyReadingsByEquipmentIdAsync(
                equipmentId,
                days * 24
            );

            // Previous period
            var previousStart = currentStart.AddDays(-days);
            var previousReadings = (await _analyticsRepository.FindEnergyReadingsByEquipmentIdAsync(
                equipmentId,
                days * 24 * 2
            )).Where(r => r.Timestamp < currentStart);

            var costAnalysis = EquipmentAnalytics.CalculateCosts(
                currentReadings.ToList(),
                previousReadings.ToList(),
                rate
            );

            return Ok(costAnalysis);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AdvancedAnalytics] Error calculating costs: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get maintenance forecast
    /// </summary>
    [HttpGet("{equipmentId:int}/maintenance-forecast")]
    [SwaggerOperation(
        Summary = "Get Maintenance Forecast",
        Description = "Predicts maintenance needs using temperature variance, trends, and equipment age (rule-based, no ML)",
        OperationId = "GetMaintenanceForecast")]
    [SwaggerResponse(StatusCodes.Status200OK, "Forecast calculated")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied")]
    public async Task<IActionResult> GetMaintenanceForecast(
        int equipmentId,
        [FromQuery] int days = 30)
    {
        var (error, _) = await VerifyOwnership(equipmentId);
        if (error != null) return error;

        try
        {
            var equipment = await _equipmentRepository.FindByIdAsync(equipmentId);
            var cutoff = DateTimeOffset.UtcNow.AddDays(-days);
            var readings = await _analyticsRepository.FindTemperatureReadingsByDateRangeAsync(
                equipmentId,
                cutoff,
                DateTimeOffset.UtcNow
            );

            var forecast = EquipmentAnalytics.ForecastMaintenance(
                readings.ToList(),
                equipment!.InstallationDate,
                equipment.OptimalTemperatureMin,
                equipment.OptimalTemperatureMax
            );

            return Ok(forecast);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AdvancedAnalytics] Error forecasting maintenance: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }
}

using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Queries;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Services;
using OsitoPolarPlatform.API.Profiles.Domain.Repositories;
using OsitoPolarPlatform.API.IAM.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using Stripe.Checkout;

namespace OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST;

/// <summary>
/// RESTful API Controller for Rental Equipment Marketplace
/// Allows Owners to browse equipment published by Providers for rent
/// </summary>
[ApiController]
[Route("api/v1/rental-equipment")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Rental Equipment Marketplace")]
public class RentalEquipmentController : ControllerBase
{
    private readonly IEquipmentQueryService _equipmentQueryService;
    private readonly IOwnerRepository _ownerRepository;
    private readonly IRenterProviderRepository _providerRepository;

    public RentalEquipmentController(
        IEquipmentQueryService equipmentQueryService,
        IOwnerRepository ownerRepository,
        IRenterProviderRepository providerRepository)
    {
        _equipmentQueryService = equipmentQueryService;
        _ownerRepository = ownerRepository;
        _providerRepository = providerRepository;
    }

    /// <summary>
    /// Get all equipment available for rent in the marketplace
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get Available Rental Equipment",
        Description = "Returns all equipment currently available for rent in the marketplace. Providers publish equipment here for Owners to rent.",
        OperationId = "GetAvailableRentalEquipment")]
    [SwaggerResponse(StatusCodes.Status200OK, "Equipment list retrieved successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request parameters")]
    public async Task<IActionResult> GetAvailableRentalEquipment(
        [FromQuery] string? type = null,
        [FromQuery] decimal? maxPrice = null)
    {
        try
        {
            Console.WriteLine($"[RentalEquipment] Getting rental equipment - type: {type}, maxPrice: {maxPrice}");

            // Query available rental equipment
            var query = new GetAvailableRentalEquipmentQuery(type, maxPrice);
            var equipments = await _equipmentQueryService.Handle(query);

            // Transform to API response
            var response = equipments.Select(e => new
            {
                id = e.Id,
                name = e.Name,
                type = e.Type.ToString(),
                model = e.Model,
                manufacturer = e.Manufacturer,
                monthlyFee = e.RentalInfo?.MonthlyFee ?? 0,
                availableFrom = e.RentalInfo?.StartDate,
                availableUntil = e.RentalInfo?.EndDate,
                providerId = e.RentalInfo?.ProviderId ?? 0,
                location = new
                {
                    name = e.Location.Name,
                    address = e.Location.Address,
                    latitude = e.Location.Coordinates.Latitude,
                    longitude = e.Location.Coordinates.Longitude
                },
                technicalDetails = e.TechnicalDetails,
                isAvailable = e.RentalInfo?.IsActive() ?? false,
                currentTemperature = e.CurrentTemperature,
                serialNumber = e.SerialNumber
            });

            Console.WriteLine($"[RentalEquipment] Returning {response.Count()} equipment items");
            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RentalEquipment] Error: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get rental equipment by ID
    /// </summary>
    [HttpGet("{equipmentId:int}")]
    [SwaggerOperation(
        Summary = "Get Rental Equipment by ID",
        Description = "Returns detailed information about specific rental equipment",
        OperationId = "GetRentalEquipmentById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Equipment found")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    public async Task<IActionResult> GetRentalEquipmentById(int equipmentId)
    {
        try
        {
            Console.WriteLine($"[RentalEquipment] Getting equipment by ID: {equipmentId}");

            var query = new GetRentalEquipmentByIdQuery(equipmentId);
            var equipment = await _equipmentQueryService.Handle(query);

            if (equipment == null)
                return NotFound(new { message = $"Equipment with ID {equipmentId} not found or not available for rent" });

            // Get provider details
            var provider = await _providerRepository.FindByIdAsync(equipment.RentalInfo!.ProviderId);

            var response = new
            {
                id = equipment.Id,
                name = equipment.Name,
                type = equipment.Type.ToString(),
                model = equipment.Model,
                manufacturer = equipment.Manufacturer,
                serialNumber = equipment.SerialNumber,
                monthlyFee = equipment.RentalInfo.MonthlyFee,
                availableFrom = equipment.RentalInfo.StartDate,
                availableUntil = equipment.RentalInfo.EndDate,
                providerId = equipment.RentalInfo.ProviderId,
                providerName = provider != null ? $"{provider.Name.FirstName} {provider.Name.LastName}" : "Unknown Provider",
                location = new
                {
                    name = equipment.Location.Name,
                    address = equipment.Location.Address,
                    latitude = equipment.Location.Coordinates.Latitude,
                    longitude = equipment.Location.Coordinates.Longitude
                },
                technicalDetails = equipment.TechnicalDetails,
                isAvailable = equipment.RentalInfo.IsActive(),
                currentTemperature = equipment.CurrentTemperature,
                setTemperature = equipment.SetTemperature,
                optimalTemperatureMin = equipment.OptimalTemperatureMin,
                optimalTemperatureMax = equipment.OptimalTemperatureMax,
                cost = equipment.Cost
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RentalEquipment] Error: {ex.Message}");
            return NotFound(new { message = $"Equipment with ID {equipmentId} not found" });
        }
    }

    /// <summary>
    /// Create rental request and Stripe checkout session (Owners only)
    /// </summary>
    [Authorize]
    [HttpPost("request")]
    [SwaggerOperation(
        Summary = "Request Equipment Rental",
        Description = "Owner creates rental request and receives Stripe checkout URL to pay.",
        OperationId = "CreateRentalRequest")]
    [SwaggerResponse(StatusCodes.Status200OK, "Checkout session created")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Equipment not available or invalid request")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Only owners can rent equipment")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Equipment not found")]
    public async Task<IActionResult> CreateRentalRequest([FromBody] CreateRentalRequestResource resource)
    {
        try
        {
            // Verify user is an Owner
            var user = (User?)HttpContext.Items["User"];
            if (user == null)
                return Unauthorized(new { message = "User not authenticated" });

            var ownerProfile = await _ownerRepository.FindByUserIdAsync(user.Id);
            if (ownerProfile == null)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Only owners can rent equipment" });

            Console.WriteLine($"[RentalRequest] Owner {ownerProfile.Id} requesting rental for equipment {resource.EquipmentId}, months: {resource.Months}");

            // Verify equipment is available for rent
            var query = new GetRentalEquipmentByIdQuery(resource.EquipmentId);
            var equipment = await _equipmentQueryService.Handle(query);

            if (equipment == null || equipment.RentalInfo == null)
                return NotFound(new { message = "Equipment not found or not available for rent" });

            if (!equipment.RentalInfo.IsActive())
                return BadRequest(new { message = "Equipment rental period is not active" });

            if (equipment.OwnerType == "Owner")
                return BadRequest(new { message = "Equipment is already rented" });

            // Calculate total cost
            var totalAmount = equipment.RentalInfo.MonthlyFee * resource.Months;
            Console.WriteLine($"[RentalRequest] Total amount: {totalAmount} ({resource.Months} months × ${equipment.RentalInfo.MonthlyFee})");

            // Create Stripe Checkout Session
            var successUrl = resource.SuccessUrl ?? "http://localhost:5173/rental/success";
            var cancelUrl = resource.CancelUrl ?? "http://localhost:5173/rental/cancel";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Rental: {equipment.Name}",
                                Description = $"{resource.Months} month(s) rental - {equipment.Type} {equipment.Model}"
                            },
                            UnitAmount = (long)(totalAmount * 100), // Convert to cents
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = $"{successUrl}?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "equipmentId", resource.EquipmentId.ToString() },
                    { "ownerId", ownerProfile.Id.ToString() },
                    { "providerId", equipment.RentalInfo.ProviderId.ToString() },
                    { "months", resource.Months.ToString() },
                    { "monthlyFee", equipment.RentalInfo.MonthlyFee.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            Console.WriteLine($"[RentalRequest] Stripe checkout session created: {session.Id}");

            return Ok(new
            {
                checkoutUrl = session.Url,
                sessionId = session.Id,
                totalAmount,
                months = resource.Months,
                monthlyFee = equipment.RentalInfo.MonthlyFee,
                equipment = new
                {
                    id = equipment.Id,
                    name = equipment.Name,
                    type = equipment.Type.ToString()
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RentalRequest] Error: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }
}

/// <summary>
/// Resource for creating rental request
/// </summary>
public record CreateRentalRequestResource
{
    public int EquipmentId { get; init; }
    public int Months { get; init; }
    public string? SuccessUrl { get; init; }
    public string? CancelUrl { get; init; }
}

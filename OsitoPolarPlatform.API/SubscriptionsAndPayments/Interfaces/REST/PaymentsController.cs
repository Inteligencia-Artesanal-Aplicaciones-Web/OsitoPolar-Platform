using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Infrastructure.External.Stripe;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Infrastructure.External.Culqi;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Infrastructure.External.Izipay;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Services;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Repositories;
using OsitoPolarPlatform.API.Profiles.Domain.Repositories;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;
using OsitoPolarPlatform.API.Notifications.Application.Internal.CommandServices;
using Swashbuckle.AspNetCore.Annotations;
using Stripe.Checkout;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Aggregates;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Payment Testing Endpoints - Test different payment providers")]
public class PaymentsController : ControllerBase
{
    private readonly StripePaymentProvider _stripeProvider;
    private readonly CulqiPaymentProvider _culqiProvider;
    private readonly IzipayPaymentProvider _izipayProvider;
    private readonly IOwnerRepository _ownerRepository;
    private readonly IRenterProviderRepository _providerRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly NotificationGeneratorService _notificationGenerator;

    // Platform commission percentage for rental transactions
    private const decimal PLATFORM_FEE_PERCENTAGE = 15.0m;

    public PaymentsController(
        StripePaymentProvider stripeProvider,
        CulqiPaymentProvider culqiProvider,
        IzipayPaymentProvider izipayProvider,
        IOwnerRepository ownerRepository,
        IRenterProviderRepository providerRepository,
        ISubscriptionRepository subscriptionRepository,
        IEquipmentRepository equipmentRepository,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        NotificationGeneratorService notificationGenerator)
    {
        _stripeProvider = stripeProvider;
        _culqiProvider = culqiProvider;
        _izipayProvider = izipayProvider;
        _ownerRepository = ownerRepository;
        _providerRepository = providerRepository;
        _subscriptionRepository = subscriptionRepository;
        _equipmentRepository = equipmentRepository;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _notificationGenerator = notificationGenerator;
    }

    /// <summary>
    /// Create Stripe Checkout Session for subscription payment
    /// </summary>
    [HttpPost("create-checkout-session")]
    [SwaggerOperation(
        Summary = "Create Stripe Checkout Session",
        Description = "Creates a Stripe Checkout Session for plan subscription",
        OperationId = "CreateCheckoutSession")]
    [SwaggerResponse(StatusCodes.Status200OK, "Checkout session created")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutSessionRequest request)
    {
        try
        {
            // Use provided amount or default to $50 for testing
            var amount = request.Amount > 0 ? request.Amount : 50.00m;

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
                                Name = $"Subscription Plan #{request.PlanId}",
                                Description = "Monthly subscription plan"
                            },
                            UnitAmount = (long)(amount * 100), // Convert to cents
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                // Add {CHECKOUT_SESSION_ID} placeholder so Stripe includes session_id in redirect
                SuccessUrl = $"{request.SuccessUrl}?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = request.CancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "userId", request.UserId.ToString() },
                    { "planId", request.PlanId.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return Ok(new
            {
                sessionId = session.Id,
                checkoutUrl = session.Url,  // Frontend expects checkoutUrl
                url = session.Url,           // Keep url for compatibility
                paymentId = session.PaymentIntentId  // Frontend expects paymentId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Verify Stripe Checkout Session status
    /// </summary>
    [HttpGet("verify/{sessionId}")]
    [SwaggerOperation(
        Summary = "Verify Checkout Session",
        Description = "Verifies the status of a Stripe Checkout Session after payment",
        OperationId = "VerifyCheckoutSession")]
    [SwaggerResponse(StatusCodes.Status200OK, "Session verified")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Session not found")]
    public async Task<IActionResult> VerifyCheckoutSession(string sessionId)
    {
        try
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);

            return Ok(new
            {
                success = true,
                paymentStatus = session.PaymentStatus,
                customerEmail = session.CustomerEmail ?? session.CustomerDetails?.Email,
                amountTotal = session.AmountTotal / 100m, // Convert from cents
                currency = session.Currency?.ToUpper(),
                metadata = session.Metadata
            });
        }
        catch (Exception ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Complete plan upgrade after successful Stripe payment
    /// </summary>
    [HttpPost("complete-upgrade")]
    [SwaggerOperation(
        Summary = "Complete Plan Upgrade",
        Description = "Verifies Stripe payment and updates user's subscription plan",
        OperationId = "CompletePlanUpgrade")]
    [SwaggerResponse(StatusCodes.Status200OK, "Plan upgraded successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Upgrade failed")]
    public async Task<IActionResult> CompletePlanUpgrade([FromBody] CompleteUpgradeRequest request)
    {
        try
        {
            Console.WriteLine($"[CompletePlanUpgrade] Processing session: {request.SessionId}");

            // 1. Verify payment with Stripe
            var service = new SessionService();
            var session = await service.GetAsync(request.SessionId);

            Console.WriteLine($"[CompletePlanUpgrade] Payment status: {session.PaymentStatus}");

            if (session.PaymentStatus != "paid")
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Payment not completed. Status: {session.PaymentStatus}"
                });
            }

            // 2. Extract metadata
            if (!session.Metadata.TryGetValue("userId", out var userIdStr) ||
                !session.Metadata.TryGetValue("planId", out var planIdStr))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Missing userId or planId in payment metadata"
                });
            }

            var userId = int.Parse(userIdStr);
            var planId = int.Parse(planIdStr);

            Console.WriteLine($"[CompletePlanUpgrade] UserId: {userId}, PlanId: {planId}");

            // 3. Get the new plan details
            var plan = await _subscriptionRepository.FindByIdAsync(planId);
            if (plan == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Plan {planId} not found"
                });
            }

            Console.WriteLine($"[CompletePlanUpgrade] Plan: {plan.PlanName}");

            // 4. Create Payment record for this subscription
            var payment = new Payment(
                userId,
                planId,
                plan.Price.Amount,
                session.Id,
                session.CustomerEmail ?? session.CustomerDetails?.Email,
                $"Subscription to {plan.PlanName}"
            );
            await _paymentRepository.AddAsync(payment);

            Console.WriteLine($"[CompletePlanUpgrade] Payment record created: {payment.Id}");

            // 5. Update Owner or Provider plan
            var owner = await _ownerRepository.FindByUserIdAsync(userId);
            if (owner != null)
            {
                // Update Owner plan
                if (!plan.MaxEquipment.HasValue)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Plan {planId} is not an Owner plan"
                    });
                }

                Console.WriteLine($"[CompletePlanUpgrade] Updating Owner {owner.Id} to plan {planId}");
                owner.UpdatePlan(planId, plan.MaxEquipment.Value);
                _ownerRepository.Update(owner);
                await _unitOfWork.CompleteAsync();

                Console.WriteLine($"[CompletePlanUpgrade] Owner plan updated successfully");

                return Ok(new
                {
                    success = true,
                    message = "Plan upgraded successfully",
                    userType = "Owner",
                    planId,
                    planName = plan.PlanName,
                    maxUnits = plan.MaxEquipment.Value,
                    transactionId = session.PaymentIntentId
                });
            }

            var provider = await _providerRepository.FindByUserIdAsync(userId);
            if (provider != null)
            {
                // Update Provider plan
                if (!plan.MaxClients.HasValue)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Plan {planId} is not a Provider plan"
                    });
                }

                Console.WriteLine($"[CompletePlanUpgrade] Updating Provider {provider.Id} to plan {planId}");
                provider.UpdatePlan(planId, plan.MaxClients.Value);
                _providerRepository.Update(provider);
                await _unitOfWork.CompleteAsync();

                Console.WriteLine($"[CompletePlanUpgrade] Provider plan updated successfully");

                return Ok(new
                {
                    success = true,
                    message = "Plan upgraded successfully",
                    userType = "Provider",
                    planId,
                    planName = plan.PlanName,
                    maxClients = plan.MaxClients.Value,
                    transactionId = session.PaymentIntentId
                });
            }

            return BadRequest(new
            {
                success = false,
                message = $"No Owner or Provider profile found for user {userId}"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CompletePlanUpgrade] Error: {ex.Message}");
            Console.WriteLine($"[CompletePlanUpgrade] Stack trace: {ex.StackTrace}");

            return BadRequest(new
            {
                success = false,
                message = $"Failed to complete upgrade: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Complete equipment rental after successful Stripe payment
    /// </summary>
    [HttpPost("complete-rental")]
    [SwaggerOperation(
        Summary = "Complete Equipment Rental",
        Description = "Verifies Stripe payment and assigns equipment to owner",
        OperationId = "CompleteEquipmentRental")]
    [SwaggerResponse(StatusCodes.Status200OK, "Rental completed successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Rental completion failed")]
    public async Task<IActionResult> CompleteEquipmentRental([FromBody] CompleteRentalRequest request)
    {
        try
        {
            Console.WriteLine($"[CompleteRental] Processing session: {request.SessionId}");

            // 1. Verify payment with Stripe
            var service = new SessionService();
            var session = await service.GetAsync(request.SessionId);

            Console.WriteLine($"[CompleteRental] Payment status: {session.PaymentStatus}");

            if (session.PaymentStatus != "paid")
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Payment not completed. Status: {session.PaymentStatus}"
                });
            }

            // 2. Extract metadata
            if (!session.Metadata.TryGetValue("equipmentId", out var equipmentIdStr) ||
                !session.Metadata.TryGetValue("ownerId", out var ownerIdStr) ||
                !session.Metadata.TryGetValue("providerId", out var providerIdStr) ||
                !session.Metadata.TryGetValue("months", out var monthsStr) ||
                !session.Metadata.TryGetValue("monthlyFee", out var monthlyFeeStr))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Missing required metadata in payment session"
                });
            }

            var equipmentId = int.Parse(equipmentIdStr);
            var ownerId = int.Parse(ownerIdStr);
            var providerId = int.Parse(providerIdStr);
            var months = int.Parse(monthsStr);
            var monthlyFee = decimal.Parse(monthlyFeeStr);

            Console.WriteLine($"[CompleteRental] EquipmentId: {equipmentId}, OwnerId: {ownerId}, ProviderId: {providerId}, Months: {months}");

            // 3. Get equipment
            var equipment = await _equipmentRepository.FindByIdAsync(equipmentId);
            if (equipment == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Equipment {equipmentId} not found"
                });
            }

            Console.WriteLine($"[CompleteRental] Equipment: {equipment.Name}");

            // 4. Calculate payment breakdown
            var totalAmount = monthlyFee * months;
            var platformFee = Math.Round(totalAmount * (PLATFORM_FEE_PERCENTAGE / 100), 2);
            var providerAmount = totalAmount - platformFee;

            Console.WriteLine($"[CompleteRental] Payment breakdown - Total: ${totalAmount}, Platform: ${platformFee}, Provider gets: ${providerAmount}");

            // 5. Update provider balance
            var provider = await _providerRepository.FindByIdAsync(providerId);
            if (provider != null)
            {
                provider.RecordServiceRevenue(providerAmount, $"Rental revenue for equipment {equipment.Name}");
                _providerRepository.Update(provider);
                Console.WriteLine($"[CompleteRental] Provider balance updated: ${provider.Balance}");
            }

            // 6. Set rental dates and assign equipment to owner
            // Calculate rental period
            var startDate = DateTimeOffset.UtcNow;
            var endDate = startDate.AddMonths(months);

            Console.WriteLine($"[CompleteRental] Setting rental period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

            // Set rental dates on the equipment
            if (equipment.RentalInfo != null)
            {
                equipment.RentalInfo.SetRentalDates(startDate, endDate);
                // Assign equipment to owner (changes owner_type from Provider to Owner)
                equipment.AssignRental(ownerId);
                _equipmentRepository.Update(equipment);
                Console.WriteLine($"[CompleteRental] Equipment rental assigned to owner {ownerId} for {months} month(s)");
            }
            else
            {
                throw new InvalidOperationException("Equipment does not have rental information");
            }

            // 7. Save all changes
            await _unitOfWork.CompleteAsync();

            // 8. Get owner details for response
            var owner = await _ownerRepository.FindByIdAsync(ownerId);

            // 9. Send notifications
            if (owner != null && provider != null)
            {
                // Notify owner about successful rental
                await _notificationGenerator.NotifyEquipmentAnomaly(
                    ownerId,
                    equipmentId,
                    equipment.Name,
                    "rental_completed",
                    $"You have successfully rented {equipment.Name} for {months} month(s). Rental ends on {endDate:yyyy-MM-dd}.");

                // Notify provider about payment received
                await _notificationGenerator.NotifyPaymentReceived(
                    provider.UserId,
                    0, // No service request for rental
                    providerAmount,
                    $"Rental payment for {equipment.Name} ({months} months)");

                Console.WriteLine($"[CompleteRental] Notifications sent to both parties");
            }

            return Ok(new
            {
                success = true,
                message = $"Equipment rented successfully for {months} month(s)",
                rental = new
                {
                    equipmentId,
                    equipmentName = equipment.Name,
                    equipmentType = equipment.Type.ToString(),
                    renterId = ownerId,
                    renterName = owner != null ? $"{owner.Name.FirstName} {owner.Name.LastName}" : "Unknown",
                    providerId,
                    providerName = provider?.CompanyName ?? "Unknown",
                    rentalStartDate = startDate,
                    rentalEndDate = endDate,
                    durationMonths = months,
                    monthlyFee = monthlyFee,
                    totalAmount,
                    platformFee,
                    providerReceived = providerAmount,
                    platformFeePercentage = PLATFORM_FEE_PERCENTAGE,
                    transactionId = session.PaymentIntentId,
                    completedAt = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CompleteRental] Error: {ex.Message}");
            Console.WriteLine($"[CompleteRental] Stack trace: {ex.StackTrace}");

            return BadRequest(new
            {
                success = false,
                message = $"Failed to complete rental: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Test Stripe payment processing
    /// </summary>
    /// <remarks>
    /// Test with Stripe test cards:
    /// - Success: 4242 4242 4242 4242
    /// - Decline: 4000 0000 0000 0002
    /// - Requires authentication: 4000 0027 6000 3184
    ///
    /// Use any future expiry date, any 3-digit CVC, any postal code
    /// </remarks>
    [HttpPost("stripe/test")]
    [SwaggerOperation(
        Summary = "Test Stripe Payment",
        Description = "Create a test payment using Stripe provider",
        OperationId = "TestStripePayment")]
    [SwaggerResponse(StatusCodes.Status200OK, "Payment processed", typeof(PaymentResult))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Payment failed")]
    public async Task<IActionResult> TestStripePayment([FromBody] TestPaymentRequest request)
    {
        var paymentRequest = new PaymentRequest
        {
            Amount = request.Amount,
            Currency = request.Currency,
            Description = request.Description,
            CustomerEmail = request.CustomerEmail,
            CustomerName = request.CustomerName,
            PaymentToken = request.PaymentToken,
            Metadata = new Dictionary<string, string>
            {
                { "test", "true" },
                { "orderId", Guid.NewGuid().ToString() }
            }
        };

        var result = await _stripeProvider.CreatePaymentAsync(paymentRequest);

        if (result.Success)
        {
            return Ok(new
            {
                success = true,
                message = "Payment successful!",
                transactionId = result.TransactionId,
                amount = result.Amount,
                currency = result.Currency,
                status = result.Status.ToString(),
                provider = "Stripe"
            });
        }

        return BadRequest(new
        {
            success = false,
            message = result.ErrorMessage,
            status = result.Status.ToString(),
            provider = "Stripe"
        });
    }

    /// <summary>
    /// Test Culqi payment processing
    /// </summary>
    [HttpPost("culqi/test")]
    [SwaggerOperation(
        Summary = "Test Culqi Payment",
        Description = "Create a test payment using Culqi provider (Peru)",
        OperationId = "TestCulqiPayment")]
    [SwaggerResponse(StatusCodes.Status200OK, "Payment processed", typeof(PaymentResult))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Payment failed")]
    public async Task<IActionResult> TestCulqiPayment([FromBody] TestPaymentRequest request)
    {
        var paymentRequest = new PaymentRequest
        {
            Amount = request.Amount,
            Currency = request.Currency,
            Description = request.Description,
            CustomerEmail = request.CustomerEmail,
            CustomerName = request.CustomerName,
            PaymentToken = request.PaymentToken,
            Metadata = new Dictionary<string, string>
            {
                { "test", "true" },
                { "orderId", Guid.NewGuid().ToString() }
            }
        };

        var result = await _culqiProvider.CreatePaymentAsync(paymentRequest);

        if (result.Success)
        {
            return Ok(new
            {
                success = true,
                message = "Payment successful!",
                transactionId = result.TransactionId,
                amount = result.Amount,
                currency = result.Currency,
                status = result.Status.ToString(),
                provider = "Culqi"
            });
        }

        return BadRequest(new
        {
            success = false,
            message = result.ErrorMessage,
            status = result.Status.ToString(),
            provider = "Culqi"
        });
    }

    /// <summary>
    /// Test Izipay payment processing
    /// </summary>
    [HttpPost("izipay/test")]
    [SwaggerOperation(
        Summary = "Test Izipay Payment",
        Description = "Create a test payment using Izipay provider (Peru)",
        OperationId = "TestIzipayPayment")]
    [SwaggerResponse(StatusCodes.Status200OK, "Payment processed", typeof(PaymentResult))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Payment failed")]
    public async Task<IActionResult> TestIzipayPayment([FromBody] TestPaymentRequest request)
    {
        var paymentRequest = new PaymentRequest
        {
            Amount = request.Amount,
            Currency = request.Currency,
            Description = request.Description,
            CustomerEmail = request.CustomerEmail,
            CustomerName = request.CustomerName,
            PaymentToken = request.PaymentToken,
            Metadata = new Dictionary<string, string>
            {
                { "test", "true" },
                { "orderId", Guid.NewGuid().ToString() }
            }
        };

        var result = await _izipayProvider.CreatePaymentAsync(paymentRequest);

        if (result.Success)
        {
            return Ok(new
            {
                success = true,
                message = "Payment successful!",
                transactionId = result.TransactionId,
                amount = result.Amount,
                currency = result.Currency,
                status = result.Status.ToString(),
                provider = "Izipay"
            });
        }

        return BadRequest(new
        {
            success = false,
            message = result.ErrorMessage,
            status = result.Status.ToString(),
            provider = "Izipay"
        });
    }

    /// <summary>
    /// Get payment status from any provider
    /// </summary>
    [HttpGet("{provider}/{transactionId}")]
    [SwaggerOperation(
        Summary = "Get Payment Status",
        Description = "Check the status of a payment by provider and transaction ID",
        OperationId = "GetPaymentStatus")]
    [SwaggerResponse(StatusCodes.Status200OK, "Payment status retrieved")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Payment not found")]
    public async Task<IActionResult> GetPaymentStatus(string provider, string transactionId)
    {
        ProviderPaymentStatus status;

        switch (provider.ToLower())
        {
            case "stripe":
                status = await _stripeProvider.GetPaymentStatusAsync(transactionId);
                break;
            case "culqi":
                status = await _culqiProvider.GetPaymentStatusAsync(transactionId);
                break;
            case "izipay":
                status = await _izipayProvider.GetPaymentStatusAsync(transactionId);
                break;
            default:
                return BadRequest(new { message = "Invalid provider. Use: stripe, culqi, or izipay" });
        }

        return Ok(new
        {
            transactionId,
            provider,
            status = status.ToString()
        });
    }
}

/// <summary>
/// Checkout session request for Stripe
/// </summary>
public record CheckoutSessionRequest
{
    public int UserId { get; init; }
    public int PlanId { get; init; }
    public decimal Amount { get; init; } = 0;  // Optional, defaults to 0 (will use $50 if not provided)
    public string SuccessUrl { get; init; } = string.Empty;
    public string CancelUrl { get; init; } = string.Empty;
}

/// <summary>
/// Complete upgrade request
/// </summary>
public record CompleteUpgradeRequest
{
    /// <summary>
    /// Stripe checkout session ID
    /// </summary>
    public string SessionId { get; init; } = string.Empty;
}

/// <summary>
/// Complete rental request
/// </summary>
public record CompleteRentalRequest
{
    /// <summary>
    /// Stripe checkout session ID
    /// </summary>
    public string SessionId { get; init; } = string.Empty;
}

/// <summary>
/// Test payment request
/// </summary>
public record TestPaymentRequest
{
    /// <summary>
    /// Amount to charge (e.g., 50.00)
    /// </summary>
    public decimal Amount { get; init; } = 50.00m;

    /// <summary>
    /// Currency code (USD, PEN, EUR)
    /// </summary>
    public string Currency { get; init; } = "USD";

    /// <summary>
    /// Payment description
    /// </summary>
    public string Description { get; init; } = "Test payment";

    /// <summary>
    /// Customer email
    /// </summary>
    public string CustomerEmail { get; init; } = "test@example.com";

    /// <summary>
    /// Customer name
    /// </summary>
    public string CustomerName { get; init; } = "Test Customer";

    /// <summary>
    /// Payment token from frontend (card token, payment method ID, etc.)
    /// For Stripe: Use payment method ID like "pm_card_visa"
    /// </summary>
    public string PaymentToken { get; init; } = "pm_card_visa";
}

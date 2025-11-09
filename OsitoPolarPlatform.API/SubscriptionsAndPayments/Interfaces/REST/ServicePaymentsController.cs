using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using OsitoPolarPlatform.API.IAM.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using OsitoPolarPlatform.API.WorkOrders.Domain.Repositories;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Repositories;
using OsitoPolarPlatform.API.Profiles.Domain.Repositories;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Services;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;
using OsitoPolarPlatform.API.Notifications.Application.Internal.CommandServices;
using Stripe;
using Stripe.Checkout;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Interfaces.REST;

/// <summary>
/// Controller for service payments (Owner pays Provider after service completion)
/// </summary>
[ApiController]
[Route("api/v1/service-payments")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Service Payments (Owner → Provider)")]
public class ServicePaymentsController : ControllerBase
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly IOwnerRepository _ownerRepository;
    private readonly IRenterProviderRepository _providerRepository;
    private readonly IServicePaymentRepository _servicePaymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly NotificationGeneratorService _notificationGenerator;
    private readonly ILogger<ServicePaymentsController> _logger;

    // Platform commission percentage
    private const decimal PLATFORM_FEE_PERCENTAGE = 15.0m; // 15%

    public ServicePaymentsController(
        IWorkOrderRepository workOrderRepository,
        IServiceRequestRepository serviceRequestRepository,
        IOwnerRepository ownerRepository,
        IRenterProviderRepository providerRepository,
        IServicePaymentRepository servicePaymentRepository,
        IUnitOfWork unitOfWork,
        NotificationGeneratorService notificationGenerator,
        ILogger<ServicePaymentsController> logger)
    {
        _workOrderRepository = workOrderRepository;
        _serviceRequestRepository = serviceRequestRepository;
        _ownerRepository = ownerRepository;
        _providerRepository = providerRepository;
        _servicePaymentRepository = servicePaymentRepository;
        _unitOfWork = unitOfWork;
        _notificationGenerator = notificationGenerator;
        _logger = logger;
    }

    /// <summary>
    /// Create payment intent for completed service (Owner pays)
    /// </summary>
    [Authorize]
    [HttpPost("create-checkout")]
    [SwaggerOperation(
        Summary = "Create Service Payment Checkout",
        Description = "Owner creates payment for completed service. Returns Stripe checkout URL.",
        OperationId = "CreateServicePaymentCheckout")]
    [SwaggerResponse(StatusCodes.Status200OK, "Checkout session created")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request or work order not resolved")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Only owners can pay for services")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Work order not found")]
    public async Task<IActionResult> CreateServicePaymentCheckout([FromBody] CreateServicePaymentResource resource)
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
                    new { message = "Only owners can pay for services" });

            _logger.LogInformation("Owner {OwnerId} creating payment for work order {WorkOrderId}",
                ownerProfile.Id, resource.WorkOrderId);

            // Get work order
            var workOrder = await _workOrderRepository.FindByIdAsync(resource.WorkOrderId);
            if (workOrder == null)
                return NotFound(new { message = "Work order not found" });

            // Verify work order is resolved and has a cost
            if (workOrder.Status != WorkOrders.Domain.Model.ValueObjects.EWorkOrderStatus.Resolved)
                return BadRequest(new { message = "Work order must be resolved before payment" });

            if (!workOrder.Cost.HasValue || workOrder.Cost.Value <= 0)
                return BadRequest(new { message = "Work order must have a valid cost" });

            // Get service request
            var serviceRequest = await _serviceRequestRepository.FindByIdAsync(workOrder.ServiceRequestId!.Value);
            if (serviceRequest == null)
                return NotFound(new { message = "Service request not found" });

            // Verify owner owns this service request
            if (serviceRequest.ClientId != ownerProfile.Id)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You can only pay for your own service requests" });

            // Get provider
            var provider = await _providerRepository.FindByIdAsync(serviceRequest.CompanyId);
            if (provider == null)
                return NotFound(new { message = "Provider not found" });

            // Calculate amounts
            var totalAmount = workOrder.Cost.Value;
            var platformFee = Math.Round(totalAmount * (PLATFORM_FEE_PERCENTAGE / 100), 2);
            var providerAmount = totalAmount - platformFee;

            _logger.LogInformation(
                "Payment breakdown - Total: ${Total}, Platform Fee: ${Fee} ({Percentage}%), Provider Gets: ${Provider}",
                totalAmount, platformFee, PLATFORM_FEE_PERCENTAGE, providerAmount);

            // Create ServicePayment record
            var servicePayment = new ServicePayment(
                workOrder.Id,
                serviceRequest.Id,
                ownerProfile.Id,
                provider.Id,
                totalAmount,
                PLATFORM_FEE_PERCENTAGE,
                $"Service payment for Work Order #{workOrder.WorkOrderNumber}");

            await _servicePaymentRepository.AddAsync(servicePayment);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("ServicePayment record created with ID: {PaymentId}", servicePayment.Id);

            // Create Stripe Checkout Session
            var successUrl = resource.SuccessUrl ?? "http://localhost:5173/payments/success";
            var cancelUrl = resource.CancelUrl ?? "http://localhost:5173/payments/cancel";

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
                                Name = $"Service Payment: {workOrder.Title}",
                                Description = $"Work Order #{workOrder.WorkOrderNumber} - {provider.CompanyName}"
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
                    { "paymentType", "service" },
                    { "servicePaymentId", servicePayment.Id.ToString() },
                    { "workOrderId", workOrder.Id.ToString() },
                    { "serviceRequestId", serviceRequest.Id.ToString() },
                    { "ownerId", ownerProfile.Id.ToString() },
                    { "providerId", provider.Id.ToString() },
                    { "totalAmount", totalAmount.ToString("F2") },
                    { "platformFee", platformFee.ToString("F2") },
                    { "providerAmount", providerAmount.ToString("F2") }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            _logger.LogInformation("Stripe checkout session created: {SessionId}", session.Id);

            return Ok(new
            {
                checkoutUrl = session.Url,
                sessionId = session.Id,
                totalAmount,
                platformFee,
                providerAmount,
                platformFeePercentage = PLATFORM_FEE_PERCENTAGE,
                workOrder = new
                {
                    id = workOrder.Id,
                    workOrderNumber = workOrder.WorkOrderNumber,
                    title = workOrder.Title,
                    providerName = provider.CompanyName
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service payment checkout");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Webhook to process service payment completion (called by Stripe)
    /// </summary>
    [HttpPost("webhook")]
    [SwaggerOperation(
        Summary = "Stripe Webhook for Service Payments",
        Description = "Processes payment completion and updates provider balance",
        OperationId = "ProcessServicePaymentWebhook")]
    public async Task<IActionResult> ProcessWebhook()
    {
        try
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json,
                Request.Headers["Stripe-Signature"],
                "your_webhook_secret"); // TODO: Move to configuration

            _logger.LogInformation("Received Stripe webhook event: {EventType}", stripeEvent.Type);

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                if (session?.Metadata?.ContainsKey("paymentType") == true &&
                    session.Metadata["paymentType"] == "service")
                {
                    await ProcessServicePaymentCompletion(session);
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return BadRequest(new { message = ex.Message });
        }
    }

    private async Task ProcessServicePaymentCompletion(Session session)
    {
        var servicePaymentId = int.Parse(session.Metadata["servicePaymentId"]);
        var workOrderId = int.Parse(session.Metadata["workOrderId"]);
        var serviceRequestId = int.Parse(session.Metadata["serviceRequestId"]);
        var ownerId = int.Parse(session.Metadata["ownerId"]);
        var providerId = int.Parse(session.Metadata["providerId"]);
        var providerAmount = decimal.Parse(session.Metadata["providerAmount"]);

        _logger.LogInformation(
            "Processing service payment completion - ServicePayment: {ServicePaymentId}, WorkOrder: {WorkOrderId}, Provider: {ProviderId}, Amount: ${Amount}",
            servicePaymentId, workOrderId, providerId, providerAmount);

        // Update ServicePayment record
        var servicePayment = await _servicePaymentRepository.FindByIdAsync(servicePaymentId);
        if (servicePayment != null)
        {
            servicePayment.MarkAsCompleted(
                session.PaymentIntentId ?? session.Id,
                session.Id);
            _servicePaymentRepository.Update(servicePayment);

            _logger.LogInformation("ServicePayment {PaymentId} marked as completed", servicePaymentId);
        }

        // Update provider balance
        var provider = await _providerRepository.FindByIdAsync(providerId);
        if (provider != null)
        {
            provider.RecordServiceRevenue(providerAmount, $"Service payment for work order #{workOrderId}");
            _providerRepository.Update(provider);

            _logger.LogInformation("Provider {ProviderId} balance updated. New balance: ${Balance}",
                providerId, provider.Balance);
        }

        // Save all changes
        await _unitOfWork.CompleteAsync();

        // Generate notification for provider
        if (provider != null)
        {
            var workOrder = await _workOrderRepository.FindByIdAsync(workOrderId);
            if (workOrder != null)
            {
                await _notificationGenerator.NotifyPaymentReceived(
                    provider.UserId,
                    serviceRequestId,
                    providerAmount,
                    workOrder.Title
                );

                _logger.LogInformation("Payment notification sent to provider {ProviderId}", providerId);
            }
        }
    }
}

/// <summary>
/// Resource for creating service payment
/// </summary>
public record CreateServicePaymentResource
{
    public int WorkOrderId { get; init; }
    public string? SuccessUrl { get; init; }
    public string? CancelUrl { get; init; }
}

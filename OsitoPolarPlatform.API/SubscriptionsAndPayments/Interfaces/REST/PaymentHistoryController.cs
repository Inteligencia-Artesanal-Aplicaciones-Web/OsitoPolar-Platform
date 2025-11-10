using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using OsitoPolarPlatform.API.IAM.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using OsitoPolarPlatform.API.Profiles.Domain.Repositories;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Repositories;
using OsitoPolarPlatform.API.WorkOrders.Domain.Repositories;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Interfaces.REST;

/// <summary>
/// Controller for payment history (Owners and Providers)
/// </summary>
[ApiController]
[Route("api/v1/payment-history")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Payment History")]
public class PaymentHistoryController : ControllerBase
{
    private readonly IServicePaymentRepository _servicePaymentRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IOwnerRepository _ownerRepository;
    private readonly IRenterProviderRepository _providerRepository;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly ILogger<PaymentHistoryController> _logger;

    public PaymentHistoryController(
        IServicePaymentRepository servicePaymentRepository,
        IPaymentRepository paymentRepository,
        ISubscriptionRepository subscriptionRepository,
        IOwnerRepository ownerRepository,
        IRenterProviderRepository providerRepository,
        IWorkOrderRepository workOrderRepository,
        ILogger<PaymentHistoryController> logger)
    {
        _servicePaymentRepository = servicePaymentRepository;
        _paymentRepository = paymentRepository;
        _subscriptionRepository = subscriptionRepository;
        _ownerRepository = ownerRepository;
        _providerRepository = providerRepository;
        _workOrderRepository = workOrderRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get payment history for the authenticated owner
    /// </summary>
    [Authorize]
    [HttpGet("owner")]
    [SwaggerOperation(
        Summary = "Get Owner Payment History",
        Description = "Returns all service payments made by the authenticated owner",
        OperationId = "GetOwnerPaymentHistory")]
    [SwaggerResponse(StatusCodes.Status200OK, "Payment history retrieved")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Only owners can view owner payment history")]
    public async Task<IActionResult> GetOwnerPaymentHistory()
    {
        try
        {
            var user = (User?)HttpContext.Items["User"];
            if (user == null)
                return Unauthorized(new { message = "User not authenticated" });

            var owner = await _ownerRepository.FindByUserIdAsync(user.Id);
            if (owner == null)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Only owners can view owner payment history" });

            _logger.LogInformation("Retrieving payment history for owner {OwnerId}", owner.Id);

            // Get subscription payments
            var subscriptionPayments = await _paymentRepository.FindByUserIdAsync(user.Id);

            // Get service payments
            var servicePayments = await _servicePaymentRepository.FindByOwnerIdAsync(owner.Id);

            var paymentHistory = new List<object>();

            // Add subscription payments
            foreach (var payment in subscriptionPayments)
            {
                var subscription = await _subscriptionRepository.FindByIdAsync(payment.SubscriptionId);

                paymentHistory.Add(new
                {
                    paymentId = payment.Id,
                    type = "Subscription",
                    description = $"Subscription: {subscription?.PlanName ?? "Plan"}",
                    totalAmount = payment.Amount.Amount,
                    platformFee = 0m,
                    providerAmount = 0m,
                    status = "Completed",
                    createdAt = DateTime.Now, // No tenemos fecha en Payment
                    completedAt = (DateTime?)null,
                    stripeSessionId = payment.StripeSession.SessionId
                });
            }

            // Add service payments
            foreach (var payment in servicePayments)
            {
                // Get work order details
                var workOrder = await _workOrderRepository.FindByIdAsync(payment.WorkOrderId);

                paymentHistory.Add(new
                {
                    paymentId = payment.Id,
                    type = "Service",
                    workOrderId = payment.WorkOrderId,
                    workOrderNumber = workOrder?.WorkOrderNumber ?? "N/A",
                    workOrderTitle = workOrder?.Title ?? "N/A",
                    serviceRequestId = payment.ServiceRequestId,
                    providerId = payment.ProviderId,
                    description = payment.Description,
                    totalAmount = payment.TotalAmount,
                    platformFee = payment.PlatformFee,
                    providerAmount = payment.ProviderAmount,
                    status = payment.Status,
                    createdAt = payment.CreatedAt,
                    completedAt = payment.CompletedAt,
                    stripePaymentIntentId = payment.StripePaymentIntentId
                });
            }

            var totalSubscriptionPayments = subscriptionPayments.Sum(p => p.Amount.Amount);
            var totalServicePayments = servicePayments.Sum(p => p.TotalAmount);
            var totalPaid = totalSubscriptionPayments + totalServicePayments;
            var totalPlatformFees = servicePayments.Sum(p => p.PlatformFee);

            return Ok(new
            {
                ownerId = owner.Id,
                ownerName = $"{owner.Name.FirstName} {owner.Name.LastName}",
                totalPayments = subscriptionPayments.Count() + servicePayments.Count(),
                totalPaid,
                totalSubscriptionPayments,
                totalServicePayments,
                totalPlatformFees,
                payments = paymentHistory.OrderByDescending(p => ((dynamic)p).createdAt).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving owner payment history");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get payment history for the authenticated provider
    /// </summary>
    [Authorize]
    [HttpGet("provider")]
    [SwaggerOperation(
        Summary = "Get Provider Payment History",
        Description = "Returns all service payments received by the authenticated provider",
        OperationId = "GetProviderPaymentHistory")]
    [SwaggerResponse(StatusCodes.Status200OK, "Payment history retrieved")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Only providers can view provider payment history")]
    public async Task<IActionResult> GetProviderPaymentHistory()
    {
        try
        {
            var user = (User?)HttpContext.Items["User"];
            if (user == null)
                return Unauthorized(new { message = "User not authenticated" });

            var provider = await _providerRepository.FindByUserIdAsync(user.Id);
            if (provider == null)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Only providers can view provider payment history" });

            _logger.LogInformation("Retrieving payment history for provider {ProviderId}", provider.Id);

            // Get subscription payments (their own subscription)
            var subscriptionPayments = await _paymentRepository.FindByUserIdAsync(user.Id);

            // Get service payments (money received from services)
            var servicePayments = await _servicePaymentRepository.FindByProviderIdAsync(provider.Id);

            var paymentHistory = new List<object>();

            // Add subscription payments (expenses)
            foreach (var payment in subscriptionPayments)
            {
                var subscription = await _subscriptionRepository.FindByIdAsync(payment.SubscriptionId);

                paymentHistory.Add(new
                {
                    paymentId = payment.Id,
                    type = "Subscription",
                    description = $"Subscription: {subscription?.PlanName ?? "Plan"}",
                    totalAmount = payment.Amount.Amount,
                    platformFee = 0m,
                    providerReceived = -payment.Amount.Amount, // Negative because it's an expense
                    status = "Completed",
                    createdAt = DateTime.Now,
                    completedAt = (DateTime?)null,
                    stripeSessionId = payment.StripeSession.SessionId
                });
            }

            // Add service payments (income)
            foreach (var payment in servicePayments)
            {
                // Get work order details
                var workOrder = await _workOrderRepository.FindByIdAsync(payment.WorkOrderId);

                paymentHistory.Add(new
                {
                    paymentId = payment.Id,
                    type = "Service",
                    workOrderId = payment.WorkOrderId,
                    workOrderNumber = workOrder?.WorkOrderNumber ?? "N/A",
                    workOrderTitle = workOrder?.Title ?? "N/A",
                    serviceRequestId = payment.ServiceRequestId,
                    ownerId = payment.OwnerId,
                    description = payment.Description,
                    totalAmount = payment.TotalAmount,
                    platformFee = payment.PlatformFee,
                    providerReceived = payment.ProviderAmount,
                    status = payment.Status,
                    createdAt = payment.CreatedAt,
                    completedAt = payment.CompletedAt,
                    stripePaymentIntentId = payment.StripePaymentIntentId
                });
            }

            var totalSubscriptionExpenses = subscriptionPayments.Sum(p => p.Amount.Amount);
            var totalReceived = servicePayments.Sum(p => p.ProviderAmount);
            var totalGrossRevenue = servicePayments.Sum(p => p.TotalAmount);
            var totalPlatformFees = servicePayments.Sum(p => p.PlatformFee);

            return Ok(new
            {
                providerId = provider.Id,
                providerName = provider.CompanyName,
                currentBalance = provider.Balance,
                totalPayments = subscriptionPayments.Count() + servicePayments.Count(),
                totalReceived, // What provider received from services (after platform fees)
                totalGrossRevenue, // Total amount paid by customers
                totalPlatformFees, // Total fees paid to platform
                totalSubscriptionExpenses, // Total spent on subscription
                netIncome = totalReceived - totalSubscriptionExpenses, // Net after expenses
                payments = paymentHistory.OrderByDescending(p => ((dynamic)p).createdAt).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving provider payment history");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get specific payment details
    /// </summary>
    [Authorize]
    [HttpGet("{paymentId:int}")]
    [SwaggerOperation(
        Summary = "Get Payment Details",
        Description = "Returns detailed information about a specific payment",
        OperationId = "GetPaymentDetails")]
    [SwaggerResponse(StatusCodes.Status200OK, "Payment details retrieved")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Payment not found")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Not authorized to view this payment")]
    public async Task<IActionResult> GetPaymentDetails(int paymentId)
    {
        try
        {
            var user = (User?)HttpContext.Items["User"];
            if (user == null)
                return Unauthorized(new { message = "User not authenticated" });

            var payment = await _servicePaymentRepository.FindByIdAsync(paymentId);
            if (payment == null)
                return NotFound(new { message = "Payment not found" });

            // Verify user is either the owner or provider involved in this payment
            var owner = await _ownerRepository.FindByUserIdAsync(user.Id);
            var provider = await _providerRepository.FindByUserIdAsync(user.Id);

            var isOwner = owner != null && owner.Id == payment.OwnerId;
            var isProvider = provider != null && provider.Id == payment.ProviderId;

            if (!isOwner && !isProvider)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Not authorized to view this payment" });

            // Get work order details
            var workOrder = await _workOrderRepository.FindByIdAsync(payment.WorkOrderId);

            return Ok(new
            {
                paymentId = payment.Id,
                workOrder = workOrder != null ? new
                {
                    id = workOrder.Id,
                    workOrderNumber = workOrder.WorkOrderNumber,
                    title = workOrder.Title,
                    description = workOrder.Description,
                    status = workOrder.Status.ToString()
                } : null,
                serviceRequestId = payment.ServiceRequestId,
                owner = new
                {
                    id = payment.OwnerId
                },
                provider = new
                {
                    id = payment.ProviderId
                },
                amounts = new
                {
                    total = payment.TotalAmount,
                    platformFee = payment.PlatformFee,
                    providerReceived = payment.ProviderAmount
                },
                status = payment.Status,
                description = payment.Description,
                createdAt = payment.CreatedAt,
                completedAt = payment.CompletedAt,
                stripePaymentIntentId = payment.StripePaymentIntentId,
                stripeTransactionId = payment.StripeTransactionId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment details");
            return BadRequest(new { message = ex.Message });
        }
    }
}

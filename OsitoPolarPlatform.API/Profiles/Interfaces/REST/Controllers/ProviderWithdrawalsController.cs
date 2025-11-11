using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using OsitoPolarPlatform.API.IAM.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using OsitoPolarPlatform.API.Profiles.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;
using OsitoPolarPlatform.API.Notifications.Interfaces.ACL;

namespace OsitoPolarPlatform.API.Profiles.Interfaces.REST.Controllers;

/// <summary>
/// Controller for provider withdrawal/payout requests
/// </summary>
[ApiController]
[Route("api/v1/provider-withdrawals")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Provider Withdrawals (Cash Out Balance)")]
public class ProviderWithdrawalsController : ControllerBase
{
    private readonly IRenterProviderRepository _providerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationContextFacade _notificationFacade;
    private readonly ILogger<ProviderWithdrawalsController> _logger;

    // Minimum withdrawal amount
    private const decimal MINIMUM_WITHDRAWAL = 10.00m;

    public ProviderWithdrawalsController(
        IRenterProviderRepository providerRepository,
        IUnitOfWork unitOfWork,
        INotificationContextFacade notificationFacade,
        ILogger<ProviderWithdrawalsController> logger)
    {
        _providerRepository = providerRepository;
        _unitOfWork = unitOfWork;
        _notificationFacade = notificationFacade;
        _logger = logger;
    }

    /// <summary>
    /// Request withdrawal of provider balance
    /// </summary>
    [Authorize]
    [HttpPost("request")]
    [SwaggerOperation(
        Summary = "Request Withdrawal",
        Description = "Provider requests to withdraw (cash out) their available balance",
        OperationId = "RequestWithdrawal")]
    [SwaggerResponse(StatusCodes.Status200OK, "Withdrawal request created")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request or insufficient balance")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Only providers can request withdrawals")]
    public async Task<IActionResult> RequestWithdrawal([FromBody] WithdrawalRequest request)
    {
        try
        {
            // Verify user is a Provider
            var user = (User?)HttpContext.Items["User"];
            if (user == null)
                return Unauthorized(new { message = "User not authenticated" });

            var provider = await _providerRepository.FindByUserIdAsync(user.Id);
            if (provider == null)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Only providers can request withdrawals" });

            _logger.LogInformation("Provider {ProviderId} requesting withdrawal of ${Amount}",
                provider.Id, request.Amount);

            // Validate withdrawal amount
            if (request.Amount < MINIMUM_WITHDRAWAL)
                return BadRequest(new
                {
                    message = $"Minimum withdrawal amount is ${MINIMUM_WITHDRAWAL}",
                    minimumAmount = MINIMUM_WITHDRAWAL
                });

            if (request.Amount > provider.Balance)
                return BadRequest(new
                {
                    message = "Insufficient balance",
                    requestedAmount = request.Amount,
                    availableBalance = provider.Balance
                });

            // Process withdrawal (deduct from balance)
            // In a real implementation, this would integrate with Stripe Payouts or similar
            // For now, we'll mark it as pending and deduct the balance
            var newBalance = provider.Balance - request.Amount;

            // Record the withdrawal transaction
            provider.RecordServiceRevenue(-request.Amount, $"Withdrawal to {request.PaymentMethod ?? "bank account"}");
            _providerRepository.Update(provider);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "Withdrawal processed for provider {ProviderId}. Amount: ${Amount}, New balance: ${Balance}",
                provider.Id, request.Amount, newBalance);

            // Generate notification using Facade
            await _notificationFacade.CreateInAppNotification(
                provider.UserId,
                "💰 Withdrawal Processed",
                $"Your withdrawal of ${request.Amount:F2} has been processed and will be sent to your account.");

            // In a real system, you would:
            // 1. Create a withdrawal record in a WithdrawalRequests table
            // 2. Initiate Stripe payout or similar
            // 3. Update withdrawal status when payout completes
            // 4. Handle failed payouts and refund balance

            return Ok(new
            {
                success = true,
                message = "Withdrawal request processed successfully",
                withdrawal = new
                {
                    amount = request.Amount,
                    previousBalance = provider.Balance + request.Amount, // Before withdrawal
                    newBalance,
                    paymentMethod = request.PaymentMethod ?? "default",
                    status = "completed", // In real system: "pending"
                    processedAt = DateTime.UtcNow,
                    estimatedArrival = "3-5 business days"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing withdrawal request");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get provider balance information
    /// </summary>
    [Authorize]
    [HttpGet("balance")]
    [SwaggerOperation(
        Summary = "Get Provider Balance",
        Description = "Returns current balance and withdrawal information for the provider",
        OperationId = "GetProviderBalance")]
    [SwaggerResponse(StatusCodes.Status200OK, "Balance retrieved")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Only providers can view balance")]
    public async Task<IActionResult> GetProviderBalance()
    {
        try
        {
            var user = (User?)HttpContext.Items["User"];
            if (user == null)
                return Unauthorized(new { message = "User not authenticated" });

            var provider = await _providerRepository.FindByUserIdAsync(user.Id);
            if (provider == null)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Only providers can view balance" });

            return Ok(new
            {
                providerId = provider.Id,
                companyName = provider.CompanyName,
                currentBalance = provider.Balance,
                availableForWithdrawal = provider.Balance,
                minimumWithdrawal = MINIMUM_WITHDRAWAL,
                canWithdraw = provider.Balance >= MINIMUM_WITHDRAWAL
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider balance");
            return BadRequest(new { message = ex.Message });
        }
    }
}

/// <summary>
/// Withdrawal request model
/// </summary>
public record WithdrawalRequest
{
    /// <summary>
    /// Amount to withdraw (must be >= minimum)
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Optional payment method identifier (bank account, card, etc.)
    /// </summary>
    public string? PaymentMethod { get; init; }
}

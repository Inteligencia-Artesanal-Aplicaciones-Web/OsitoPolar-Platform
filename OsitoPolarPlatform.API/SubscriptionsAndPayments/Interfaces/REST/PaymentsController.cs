using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Commands;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Services;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Interfaces.REST.Resources;
using Swashbuckle.AspNetCore.Annotations;
using Stripe;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Payment Management Endpoints")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentCommandService _paymentCommandService;
    private readonly IStripeService _stripeService;
    private readonly IConfiguration _configuration;

    public PaymentsController(
        IPaymentCommandService paymentCommandService,
        IStripeService stripeService,
        IConfiguration configuration)
    {
        _paymentCommandService = paymentCommandService;
        _stripeService = stripeService;
        _configuration = configuration;
    }

    /// <summary>
    /// Creates a Stripe checkout session for subscription payment
    /// </summary>
    [HttpPost("create-checkout-session")]
    [SwaggerOperation(
        Summary = "Create Checkout Session",
        Description = "Creates a Stripe checkout session for subscription payment",
        OperationId = "CreateCheckoutSession")]
    [SwaggerResponse(StatusCodes.Status200OK, "Checkout session created", typeof(CreateCheckoutSessionResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionResource resource)
    {
        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var successUrl = resource.SuccessUrl ?? $"{baseUrl}/plans?session_id={{CHECKOUT_SESSION_ID}}&plan_id={resource.PlanId}";
            var cancelUrl = resource.CancelUrl ?? $"{baseUrl}/plans?canceled=true";

            var command = new CreatePaymentSessionCommand(
                resource.UserId,
                resource.PlanId,
                successUrl,
                cancelUrl
            );

            var (payment, checkoutUrl) = await _paymentCommandService.Handle(command);

            return Ok(new CreateCheckoutSessionResponse(
                payment.Id,
                checkoutUrl,
                payment.StripeSession.SessionId
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Webhook endpoint for Stripe events
    /// </summary>
    [HttpPost("webhook")]
    [SwaggerOperation(
        Summary = "Stripe Webhook",
        Description = "Processes Stripe webhook events",
        OperationId = "StripeWebhook")]
    [SwaggerResponse(StatusCodes.Status200OK, "Webhook processed")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"];

        if (!await _stripeService.ValidateWebhookSignatureAsync(json, stripeSignature))
        {
            return BadRequest("Invalid signature");
        }

        try
        {
            var stripeEvent = EventUtility.ParseEvent(json);
            
            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                if (session != null)
                {
                    var command = new ProcessPaymentWebhookCommand(session.Id, stripeEvent.Type);
                    await _paymentCommandService.Handle(command);
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public record CreateCheckoutSessionResource(int UserId, int PlanId, string? SuccessUrl = null, string? CancelUrl = null);
public record CreateCheckoutSessionResponse(int PaymentId, string CheckoutUrl, string SessionId);
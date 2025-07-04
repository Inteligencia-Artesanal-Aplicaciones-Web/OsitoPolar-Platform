using Microsoft.Extensions.Options;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Services;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Infrastructure.External.Configuration;
using Stripe;
using Stripe.Checkout;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Infrastructure.External.Services;

public class StripeService : IStripeService
{
    private readonly StripeSettings _stripeSettings;
    private readonly Dictionary<int, string> _priceIds = new()
    {
        { 1, "price_1RbjtZIpwjcJDlEvwENLApxN" }, // Basic (Polar Bear)
        { 2, "price_1RbkNFIpwjcJDlEvSDiPeGM2" }, // Standard (Snow Bear)
        { 3, "price_1RbjvyIpwjcJDlEvU7q0oDnS" }, // Premium (Glacial Bear)
        { 4, "price_1RbjwoIpwjcJDlEvqkqCJqUs" }, // Small Company
        { 5, "price_1RbjxLIpwjcJDlEvCGicb8wf" }, // Medium Company
        { 6, "price_1RbjxpIpwjcJDlEvV1pbNbYs" }  // Enterprise Premium
    };

    public StripeService(IOptions<StripeSettings> stripeSettings)
    {
        _stripeSettings = stripeSettings.Value;
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
    }

    public async Task<string> CreateCheckoutSessionAsync(int planId, string successUrl, string cancelUrl, string? customerEmail = null)
    {
        if (!_priceIds.TryGetValue(planId, out var priceId))
            throw new ArgumentException($"No Stripe price ID found for plan {planId}");

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1,
                },
            },
            Mode = "subscription",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            CustomerEmail = customerEmail,
            Metadata = new Dictionary<string, string>
            {
                { "planId", planId.ToString() }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);
        
        return session.Url;
    }

    public async Task<bool> ValidateWebhookSignatureAsync(string payload, string signature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                payload,
                signature,
                _stripeSettings.WebhookSecret
            );
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(string sessionId, string status, string? customerEmail)> GetSessionDetailsAsync(string sessionId)
    {
        var service = new SessionService();
        var session = await service.GetAsync(sessionId);
        
        return (session.Id, session.PaymentStatus, session.CustomerEmail);
    }
}
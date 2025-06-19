using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Aggregates;

/// <summary>
/// Represents a subscription plan for a user or provider.
/// </summary>
public partial class Subscription
{
    public int Id { get; private set; }
    public string PlanName { get; private set; }
    public Price Price { get; private set; }
    public BillingCycle BillingCycle { get; private set; }
    public int? MaxEquipment { get; private set; }
    public int? MaxClients { get; private set; }
    public List<Feature> Features { get; private set; }

    protected Subscription()
    {
        PlanName = string.Empty;
        Price = new Price(0m, "USD"); // Default to 0, will be set by constructor
        BillingCycle = BillingCycle.Monthly; // Default to Monthly based on db.json
        Features = new List<Feature>();
    }

    public Subscription(int id, string planName, decimal price, BillingCycle billingCycle, int? maxEquipment = null, int? maxClients = null, List<string>? featureNames = null) : this()
    {
        Id = id;
        PlanName = planName ?? throw new ArgumentNullException(nameof(planName));
        Price = new Price(price, "USD");
        BillingCycle = billingCycle;
        MaxEquipment = maxEquipment;
        MaxClients = maxClients;
        Features = featureNames?.Select(f => new Feature(f)).ToList() ?? new List<Feature>();
    }

    public void UpdatePlan(string newPlanName, decimal newPrice, BillingCycle newBillingCycle, int? newMaxEquipment = null, int? newMaxClients = null, List<string>? newFeatureNames = null)
    {
        PlanName = newPlanName;
        Price = new Price(newPrice, "USD");
        BillingCycle = newBillingCycle;
        MaxEquipment = newMaxEquipment;
        MaxClients = newMaxClients;
        Features = newFeatureNames?.Select(f => new Feature(f)).ToList() ?? Features;
    }
}
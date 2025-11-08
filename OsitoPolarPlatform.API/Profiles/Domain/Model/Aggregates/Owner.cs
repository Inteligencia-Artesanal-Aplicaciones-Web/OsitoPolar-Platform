namespace OsitoPolarPlatform.API.Profiles.Domain.Model.Aggregates;

/// <summary>
/// Owner aggregate - Represents a business that owns/rents equipment
/// Examples: Restaurants, Hotels, Supermarkets, Laboratories
/// </summary>
public class Owner : Profile
{
    /// <summary>
    /// Link to IAM User (for authentication)
    /// </summary>
    public int UserId { get; private set; }

    /// <summary>
    /// Current balance (negative = owes money, positive = credit)
    /// Tracks: subscription fees + service charges from providers
    /// </summary>
    public decimal Balance { get; private set; }

    /// <summary>
    /// Subscription plan ID (Polar Bear, Snow Bear, Glacial Bear)
    /// Links to SubscriptionsAndPayments BC
    /// </summary>
    public int PlanId { get; private set; }

    /// <summary>
    /// Maximum number of equipment units allowed by plan
    /// </summary>
    public int MaxUnits { get; private set; }

    /// <summary>
    /// EF Core constructor
    /// </summary>
    public Owner() : base()
    {
    }

    /// <summary>
    /// Create a new Owner
    /// </summary>
    public Owner(
        int userId,
        string firstName,
        string lastName,
        string email,
        string street,
        string number,
        string city,
        string postalCode,
        string country,
        int planId,
        int maxUnits)
        : base(firstName, lastName, email, street, number, city, postalCode, country)
    {
        UserId = userId;
        Balance = 0.00m; // Start with zero balance
        PlanId = planId;
        MaxUnits = maxUnits;
    }

    /// <summary>
    /// Add charge to balance (subscription fee, service fee, etc.)
    /// </summary>
    public void AddCharge(decimal amount, string reason)
    {
        if (amount <= 0)
            throw new ArgumentException("Charge amount must be positive");

        Balance -= amount; // Negative balance = owes money
    }

    /// <summary>
    /// Record payment (reduces debt)
    /// </summary>
    public void RecordPayment(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be positive");

        Balance += amount; // Increases balance (less debt)
    }

    /// <summary>
    /// Update subscription plan
    /// </summary>
    public void UpdatePlan(int planId, int maxUnits)
    {
        PlanId = planId;
        MaxUnits = maxUnits;
    }
}

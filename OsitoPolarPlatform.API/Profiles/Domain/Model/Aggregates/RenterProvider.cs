namespace OsitoPolarPlatform.API.Profiles.Domain.Model.Aggregates;

/// <summary>
/// RenterProvider aggregate - Represents a technician company that provides services
/// Examples: Samsung, LG, Bosch, Local technician companies
/// </summary>
public class RenterProvider : Profile
{
    /// <summary>
    /// Link to IAM User (for authentication)
    /// </summary>
    public int UserId { get; private set; }

    /// <summary>
    /// Current balance (positive = earnings, negative = owes subscription)
    /// Tracks: service fees earned - subscription costs
    /// </summary>
    public decimal Balance { get; private set; }

    /// <summary>
    /// Subscription plan ID (Small Company, Medium Company, Enterprise Premium)
    /// Links to SubscriptionsAndPayments BC
    /// </summary>
    public int PlanId { get; private set; }

    /// <summary>
    /// Maximum number of clients allowed by plan
    /// </summary>
    public int MaxClients { get; private set; }

    /// <summary>
    /// Company name (e.g., "Samsung Refrigeration Services")
    /// </summary>
    public string CompanyName { get; private set; }

    /// <summary>
    /// Tax ID / Business registration number
    /// </summary>
    public string? TaxId { get; private set; }

    /// <summary>
    /// EF Core constructor
    /// </summary>
    public RenterProvider() : base()
    {
        CompanyName = string.Empty;
    }

    /// <summary>
    /// Create a new RenterProvider
    /// </summary>
    public RenterProvider(
        int userId,
        string companyName,
        string contactFirstName,
        string contactLastName,
        string email,
        string street,
        string number,
        string city,
        string postalCode,
        string country,
        int planId,
        int maxClients,
        string? taxId = null)
        : base(contactFirstName, contactLastName, email, street, number, city, postalCode, country)
    {
        UserId = userId;
        CompanyName = companyName;
        Balance = 0.00m; // Start with zero balance
        PlanId = planId;
        MaxClients = maxClients;
        TaxId = taxId;
    }

    /// <summary>
    /// Record service fee earned from an Owner
    /// </summary>
    public void RecordServiceRevenue(decimal amount, string serviceDescription)
    {
        if (amount <= 0)
            throw new ArgumentException("Revenue amount must be positive");

        Balance += amount; // Increase balance (earnings)
    }

    /// <summary>
    /// Charge subscription fee
    /// </summary>
    public void ChargeSubscriptionFee(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Subscription fee must be positive");

        Balance -= amount; // Decrease balance (cost)
    }

    /// <summary>
    /// Record withdrawal (provider withdraws earnings)
    /// </summary>
    public void RecordWithdrawal(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive");

        if (Balance < amount)
            throw new InvalidOperationException($"Insufficient balance. Available: {Balance}, Requested: {amount}");

        Balance -= amount;
    }

    /// <summary>
    /// Update subscription plan
    /// </summary>
    public void UpdatePlan(int planId, int maxClients)
    {
        PlanId = planId;
        MaxClients = maxClients;
    }

    /// <summary>
    /// Update company information
    /// </summary>
    public void UpdateCompanyInfo(string companyName, string? taxId)
    {
        CompanyName = companyName;
        TaxId = taxId;
    }
}

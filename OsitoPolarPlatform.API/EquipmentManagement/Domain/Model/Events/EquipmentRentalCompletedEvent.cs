using OsitoPolarPlatform.API.Shared.Domain.Model.Events;

namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Events;

/// <summary>
/// Domain event raised when an equipment rental is completed (payment processed and equipment assigned)
/// </summary>
public record EquipmentRentalCompletedEvent : IEvent
{
    public int EquipmentId { get; init; }
    public string EquipmentName { get; init; }
    public string EquipmentType { get; init; }
    public int OwnerId { get; init; } // The renter (owner who is renting the equipment)
    public int ProviderId { get; init; } // The equipment provider
    public int RentalDurationMonths { get; init; }
    public decimal MonthlyFee { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal PlatformFee { get; init; }
    public decimal ProviderAmount { get; init; }
    public DateTimeOffset RentalStartDate { get; init; }
    public DateTimeOffset RentalEndDate { get; init; }
    public string StripeSessionId { get; init; }
    public DateTime OccurredAt { get; init; }

    public EquipmentRentalCompletedEvent(
        int equipmentId,
        string equipmentName,
        string equipmentType,
        int ownerId,
        int providerId,
        int rentalDurationMonths,
        decimal monthlyFee,
        decimal totalAmount,
        decimal platformFee,
        decimal providerAmount,
        DateTimeOffset rentalStartDate,
        DateTimeOffset rentalEndDate,
        string stripeSessionId)
    {
        EquipmentId = equipmentId;
        EquipmentName = equipmentName ?? throw new ArgumentNullException(nameof(equipmentName));
        EquipmentType = equipmentType ?? throw new ArgumentNullException(nameof(equipmentType));
        OwnerId = ownerId;
        ProviderId = providerId;
        RentalDurationMonths = rentalDurationMonths;
        MonthlyFee = monthlyFee;
        TotalAmount = totalAmount;
        PlatformFee = platformFee;
        ProviderAmount = providerAmount;
        RentalStartDate = rentalStartDate;
        RentalEndDate = rentalEndDate;
        StripeSessionId = stripeSessionId ?? throw new ArgumentNullException(nameof(stripeSessionId));
        OccurredAt = DateTime.UtcNow;
    }
}

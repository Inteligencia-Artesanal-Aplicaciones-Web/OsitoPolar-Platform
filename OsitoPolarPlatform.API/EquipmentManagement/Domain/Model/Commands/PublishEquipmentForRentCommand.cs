namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Commands;

/// <summary>
/// Command to publish equipment for rent in the marketplace
/// </summary>
public record PublishEquipmentForRentCommand(
    int EquipmentId,
    int ProviderId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal MonthlyFee
);

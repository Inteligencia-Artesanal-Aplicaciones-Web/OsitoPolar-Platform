namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Queries;

/// <summary>
/// Query to get equipment available for rent in the marketplace
/// </summary>
public record GetAvailableRentalEquipmentQuery(
    string? Type = null,
    decimal? MaxMonthlyFee = null
);

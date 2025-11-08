namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Commands;

/// <summary>
/// Command to remove equipment from rental marketplace
/// </summary>
public record UnpublishEquipmentFromRentCommand(int EquipmentId, int ProviderId);

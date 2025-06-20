namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Commands;

/// <summary>
/// Command for deleting equipment from the system
/// </summary>
/// <param name="EquipmentId">The unique identifier of the equipment to delete</param>
public record DeleteEquipmentCommand(int EquipmentId);

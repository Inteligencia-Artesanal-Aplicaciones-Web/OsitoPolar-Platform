namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.ValueObjects;

/// <summary>
/// Represents the operational status of equipment.
/// </summary>
public enum EEquipmentStatus
{
    Active,
    Inactive,
    Maintenance,
    OutOfService
}
namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.ValueObjects;

/// <summary>
/// Represents a unique identifier for equipment in the Osito Polar Platform.
/// </summary>
/// <param name="Identifier">The unique identifier for the equipment</param>
public record EquipmentIdentifier(Guid Identifier)
{
    /// <summary>
    /// Initializes a new instance with a new GUID.
    /// </summary>
    public EquipmentIdentifier() : this(Guid.NewGuid()) { }
}
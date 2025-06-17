namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.ValueObjects;

/// <summary>
/// Represents the energy consumption of equipment.
/// </summary>
/// <param name="Current">The current energy consumption value.</param>
/// <param name="Unit">The unit of measurement for energy consumption.</param>
/// <param name="Average">The average energy consumption value.</param>
/// 
public record EnergyConsumption(decimal Current, string Unit, decimal Average);
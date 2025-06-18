namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.ValueObjects;

/// <summary>
/// Represents geographical coordinates for equipment location.
/// </summary>
/// <param name="Latitude">The latitude coordinate</param>
/// <param name="Longitude">The longitude coordinate</param>
public record Coordinates(decimal Latitude, decimal Longitude);
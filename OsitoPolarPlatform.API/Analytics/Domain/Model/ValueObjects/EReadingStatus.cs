namespace OsitoPolarPlatform.API.Analytics.Domain.Model.ValueObjects;

/// <summary>
/// Represents the status of a reading (temperature, energy, etc.)
/// </summary>
public enum EReadingStatus
{
    Normal,
    Warning,
    Critical
}
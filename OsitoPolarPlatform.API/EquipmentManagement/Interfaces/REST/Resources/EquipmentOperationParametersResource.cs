using System.ComponentModel.DataAnnotations;

namespace OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Resources;

/// <summary>
/// Resource for unified equipment operation parameters update
/// Based on real domain: refrigeration equipment operations
/// </summary>
public class EquipmentOperationParametersResource
{
    /// <summary>
    /// Equipment target temperature setting in Celsius
    /// Maps to equipment.setTemperature in your domain
    /// </summary>
    [Range(-50, 50, ErrorMessage = "Temperature must be between -50°C and 50°C")]
    public decimal? Temperature { get; set; }

    /// <summary>
    /// Equipment power state (ON, OFF, STANDBY)  
    /// Maps to equipment.isPoweredOn in your domain
    /// </summary>
    [RegularExpression("^(ON|OFF|STANDBY)$", ErrorMessage = "Power state must be ON, OFF, or STANDBY")]
    public string? PowerState { get; set; }

    /// <summary>
    /// Equipment location information
    /// Maps to equipment.location in your domain
    /// </summary>
    public LocationResource? Location { get; set; }

    /// <summary>
    /// Equipment operational status from your domain
    /// Maps to equipment.status (active, inactive, maintenance)
    /// </summary>
    [RegularExpression("^(active|inactive|maintenance)$", 
        ErrorMessage = "Status must be active, inactive, or maintenance")]
    public string? Status { get; set; }

    /// <summary>
    /// Equipment maintenance notes
    /// Maps to equipment.notes in your domain
    /// </summary>
    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// Resource for location information
/// </summary>
public class LocationResource
{
    [Required]
    [StringLength(100, ErrorMessage = "Address cannot exceed 100 characters")]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public double Latitude { get; set; }

    [Required]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public double Longitude { get; set; }

    [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
    public string? City { get; set; }

    [StringLength(50, ErrorMessage = "Country cannot exceed 50 characters")]
    public string? Country { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Resources;

/// <summary>
/// Resource for creating equipment readings
/// Replaces CreateTemperatureReadingResource and CreateEnergyReadingResource
/// </summary>
public class CreateEquipmentReadingResource
{
    [Required]
    [RegularExpression("^(temperature|energy)$", 
        ErrorMessage = "Type must be temperature or energy")]
    public string Type { get; set; } = string.Empty;

    [Required]
    public decimal Value { get; set; }

    [Required]
    [StringLength(20, ErrorMessage = "Unit cannot exceed 20 characters")]
    public string Unit { get; set; } = string.Empty; // "celsius", "watts"

    public DateTimeOffset? Timestamp { get; set; } = null;

    [RegularExpression("^(normal|warning|critical)$", 
        ErrorMessage = "Status must be normal, warning, or critical")]
    public string Status { get; set; } = "normal";

    [StringLength(200, ErrorMessage = "Notes cannot exceed 200 characters")]
    public string? Notes { get; set; }
}
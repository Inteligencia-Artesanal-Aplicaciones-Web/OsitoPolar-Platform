using System.ComponentModel.DataAnnotations;

namespace OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Resources;

/// <summary>
/// Complete equipment update resource 
/// </summary>
public class UpdateEquipmentResource
{
    [Required]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
    public string Type { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [Range(-50, 50, ErrorMessage = "Temperature must be between -50°C and 50°C")]
    public decimal? Temperature { get; set; }

    [RegularExpression("^(ON|OFF|STANDBY)$", ErrorMessage = "Power state must be ON, OFF, or STANDBY")]
    public string? PowerState { get; set; }

    public LocationResource? Location { get; set; }

    [Required]
    public int OwnerId { get; set; }

    public bool IsRented { get; set; }
}
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Commands;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.ValueObjects;
// Remove the confusing aliases and use the ValueObjects directly

namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Aggregates;

/// <summary>
/// Represents a refrigeration equipment aggregate root.
/// </summary>
public partial class Equipment
{
    public int Id { get; private set; }
    public EquipmentIdentifier EquipmentIdentifier { get; private set; }
    public string Name { get; private set; }
    public EEquipmentType Type { get; private set; }
    public string Model { get; private set; }
    public string Manufacturer { get; private set; }
    public string SerialNumber { get; private set; }
    public string Code { get; private set; }
    public decimal Cost { get; private set; }
    public string TechnicalDetails { get; private set; }
    public EEquipmentStatus Status { get; private set; }
    public bool IsPoweredOn { get; private set; }
    public DateTimeOffset InstallationDate { get; private set; }
    public decimal CurrentTemperature { get; private set; }
    public decimal SetTemperature { get; private set; }
    public decimal OptimalTemperatureMin { get; private set; }
    public decimal OptimalTemperatureMax { get; private set; }
    public Entities.Location Location { get; private set; }
    public Entities.EnergyConsumption EnergyConsumption { get; private set; }
    public int OwnerId { get; private set; }
    public string OwnerType { get; private set; }
    public EOwnershipType OwnershipType { get; private set; }
    public Entities.RentalInfo? RentalInfo { get; private set; }
    public string Notes { get; private set; }

    protected Equipment()
    {
        EquipmentIdentifier = new EquipmentIdentifier();
        Name = string.Empty;
        Model = string.Empty;
        Manufacturer = string.Empty;
        SerialNumber = string.Empty;
        Code = string.Empty;
        TechnicalDetails = string.Empty;
        Status = EEquipmentStatus.Active;
        IsPoweredOn = true;
        InstallationDate = DateTimeOffset.UtcNow;
        Location = new Entities.Location();
        EnergyConsumption = new Entities.EnergyConsumption();
        OwnerType = string.Empty;
        OwnershipType = EOwnershipType.Owned;
        Notes = string.Empty;
    }

    public Equipment(CreateEquipmentCommand command) : this()
    {
        Name = command.Name;
        Type = Enum.Parse<EEquipmentType>(command.Type, true);
        Model = command.Model;
        Manufacturer = command.Manufacturer;
        SerialNumber = command.SerialNumber;
        Code = command.Code;
        Cost = command.Cost;
        TechnicalDetails = command.TechnicalDetails;
        CurrentTemperature = command.CurrentTemperature;
        SetTemperature = command.SetTemperature;
        OptimalTemperatureMin = command.OptimalTemperatureMin;
        OptimalTemperatureMax = command.OptimalTemperatureMax;
        Location = new Entities.Location(command.LocationName, command.LocationAddress, 
                               command.LocationLatitude, command.LocationLongitude);
        EnergyConsumption = new Entities.EnergyConsumption(command.EnergyConsumptionCurrent, 
            command.EnergyConsumptionUnit, 
            command.EnergyConsumptionAverage);
        OwnerId = command.OwnerId;
        OwnerType = command.OwnerType;
        OwnershipType = Enum.Parse<EOwnershipType>(command.OwnershipType, true);
        Notes = command.Notes;
    }
}
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Commands;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Aggregates;

public partial class Equipment
{
    public void UpdateTemperature(decimal newTemperature)
    {
        if (!IsPoweredOn)
            throw new InvalidOperationException("Cannot update temperature when equipment is powered off");

        SetTemperature = newTemperature;
    }

    public void UpdatePowerState(bool isPoweredOn)
    {
        IsPoweredOn = isPoweredOn;
        
        if (!isPoweredOn)
        {
            Status = EEquipmentStatus.Inactive;
        }
        else if (Status == EEquipmentStatus.Inactive)
        {
            Status = EEquipmentStatus.Active;
        }
    }

    public void UpdateLocation(string locationName, string locationAddress, decimal latitude, decimal longitude)
    {
        Location.UpdateLocation(locationName, locationAddress, latitude, longitude);
    }

    public void UpdateStatus(EEquipmentStatus newStatus)
    {
        Status = newStatus;
    }

    public string GetTemperatureStatus()
    {
        if (CurrentTemperature < OptimalTemperatureMin || CurrentTemperature > OptimalTemperatureMax)
        {
            var minDiff = Math.Abs(CurrentTemperature - OptimalTemperatureMin);
            var maxDiff = Math.Abs(CurrentTemperature - OptimalTemperatureMax);
            var threshold = (OptimalTemperatureMax - OptimalTemperatureMin) * 0.2m;

            if (minDiff > threshold || maxDiff > threshold)
                return "critical";
            
            return "warning";
        }
        return "normal";
    }

    public string GetStatusColor()
    {
        return GetTemperatureStatus() switch
        {
            "critical" => "#FF5252",
            "warning" => "#FFC107",
            _ => "#4CAF50"
        };
    }

    public string GetTypeDisplay()
    {
        return Type switch
        {
            EEquipmentType.Freezer => "Freezer",
            EEquipmentType.ColdRoom => "Cold Room",
            EEquipmentType.Refrigerator => "Refrigerator",
            _ => Type.ToString()
        };
    }

    public void Handle(UpdateEquipmentTemperatureCommand command)
    {
        if (command.EquipmentId == Id)
            UpdateTemperature(command.NewTemperature);
    }

    public void Handle(UpdateEquipmentPowerStateCommand command)
    {
        if (command.EquipmentId == Id)
            UpdatePowerState(command.IsPoweredOn);
    }

    public void Handle(UpdateEquipmentLocationCommand command)
    {
        if (command.EquipmentId == Id)
            UpdateLocation(command.LocationName, command.LocationAddress, 
                          command.Latitude, command.Longitude);
    }
}
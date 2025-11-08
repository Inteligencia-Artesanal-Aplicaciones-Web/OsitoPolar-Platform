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

    // ========== RENTAL EQUIPMENT METHODS ==========

    /// <summary>
    /// Provider publishes equipment for rent in the marketplace
    /// </summary>
    public void PublishForRent(DateTimeOffset startDate, DateTimeOffset endDate, decimal monthlyFee, int providerId)
    {
        if (OwnerType != "Provider")
            throw new InvalidOperationException("Only providers can publish equipment for rent");

        if (RentalInfo != null && RentalInfo.IsActive())
            throw new InvalidOperationException("Equipment is currently rented and cannot be published");

        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");

        if (monthlyFee <= 0)
            throw new ArgumentException("Monthly fee must be positive");

        RentalInfo = new Entities.RentalInfo(startDate, endDate, monthlyFee, providerId);
        OwnershipType = EOwnershipType.Rented;
    }

    /// <summary>
    /// Provider removes equipment from rental marketplace
    /// </summary>
    public void UnpublishFromRent()
    {
        if (RentalInfo == null)
            throw new InvalidOperationException("Equipment is not published for rent");

        if (OwnerType == "Owner")
            throw new InvalidOperationException("Cannot unpublish equipment that is currently rented by an owner");

        RentalInfo = null;
        OwnershipType = EOwnershipType.Owned;
    }

    /// <summary>
    /// Assign rental to an owner after successful payment
    /// </summary>
    public void AssignRental(int ownerId)
    {
        if (RentalInfo == null)
            throw new InvalidOperationException("Equipment is not available for rent");

        if (!RentalInfo.IsActive())
            throw new InvalidOperationException("Rental period is not active");

        if (OwnerType == "Owner")
            throw new InvalidOperationException("Equipment is already rented");

        OwnerId = ownerId;
        OwnerType = "Owner";
    }

    /// <summary>
    /// Return equipment to provider after rental period ends
    /// </summary>
    public void ReturnFromRental(int providerId)
    {
        if (RentalInfo == null)
            throw new InvalidOperationException("Equipment does not have rental information");

        if (OwnerType != "Owner")
            throw new InvalidOperationException("Equipment is not currently rented by an owner");

        OwnerId = providerId;
        OwnerType = "Provider";
        RentalInfo = null;
        OwnershipType = EOwnershipType.Owned;
    }
}
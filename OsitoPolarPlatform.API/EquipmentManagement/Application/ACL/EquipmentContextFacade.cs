using OsitoPolarPlatform.API.EquipmentManagement.Domain.Repositories;
using OsitoPolarPlatform.API.EquipmentManagement.Interfaces.ACL;

namespace OsitoPolarPlatform.API.EquipmentManagement.Application.ACL;

/// <summary>
/// Facade implementation for the Equipment Management context
/// </summary>
/// <param name="equipmentRepository">The equipment repository</param>
public class EquipmentContextFacade(IEquipmentRepository equipmentRepository) : IEquipmentContextFacade
{
    public async Task<bool> EquipmentExists(int equipmentId)
    {
        var equipment = await equipmentRepository.FindByIdAsync(equipmentId);
        return equipment != null;
    }

    public async Task<string> FetchEquipmentOwnerType(int equipmentId)
    {
        var equipment = await equipmentRepository.FindByIdAsync(equipmentId);
        return equipment?.OwnerType ?? string.Empty;
    }

    public async Task<int> FetchEquipmentOwnerId(int equipmentId)
    {
        var equipment = await equipmentRepository.FindByIdAsync(equipmentId);
        return equipment?.OwnerId ?? 0;
    }

    public async Task<bool> IsEquipmentOwnedBy(int equipmentId, int ownerId)
    {
        var equipment = await equipmentRepository.FindByIdAsync(equipmentId);
        if (equipment == null) return false;

        return equipment.OwnerId == ownerId;
    }

    public async Task<IEnumerable<int>> FetchEquipmentIdsByOwnerId(int ownerId)
    {
        var equipments = await equipmentRepository.FindByOwnerIdAsync(ownerId);
        return equipments.Select(e => e.Id);
    }

    public async Task<int> CountEquipmentByOwnerId(int ownerId)
    {
        var equipments = await equipmentRepository.FindByOwnerIdAsync(ownerId);
        return equipments.Count();
    }

    public async Task<(decimal minTemp, decimal maxTemp)?> GetEquipmentOptimalTemperatureRange(int equipmentId)
    {
        var equipment = await equipmentRepository.FindByIdAsync(equipmentId);
        if (equipment == null) return null;

        return (equipment.OptimalTemperatureMin, equipment.OptimalTemperatureMax);
    }

    public async Task<(DateTimeOffset installationDate, decimal minTemp, decimal maxTemp)?> GetEquipmentMaintenanceData(int equipmentId)
    {
        var equipment = await equipmentRepository.FindByIdAsync(equipmentId);
        if (equipment == null) return null;

        return (equipment.InstallationDate, equipment.OptimalTemperatureMin, equipment.OptimalTemperatureMax);
    }

    public async Task<(int id, string name, string type, bool hasRentalInfo)?> GetEquipmentRentalData(int equipmentId)
    {
        var equipment = await equipmentRepository.FindByIdAsync(equipmentId);
        if (equipment == null) return null;

        return (equipment.Id, equipment.Name, equipment.Type.ToString(), equipment.RentalInfo != null);
    }

    public async Task<bool> ProcessEquipmentRental(int equipmentId, int ownerId, DateTimeOffset startDate, DateTimeOffset endDate)
    {
        var equipment = await equipmentRepository.FindByIdAsync(equipmentId);
        if (equipment == null || equipment.RentalInfo == null) return false;

        equipment.RentalInfo.SetRentalDates(startDate, endDate);
        equipment.AssignRental(ownerId);
        equipmentRepository.Update(equipment);
        return true;
    }
}

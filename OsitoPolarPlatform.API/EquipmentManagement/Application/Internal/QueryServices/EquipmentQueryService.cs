using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Queries;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Repositories;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Services;

namespace OsitoPolarPlatform.API.EquipmentManagement.Application.Internal.QueryServices;

/// <summary>
/// Concrete implementation of IEquipmentQueryService.
/// </summary>
public class EquipmentQueryService(IEquipmentRepository equipmentRepository) : IEquipmentQueryService
{
    public async Task<Equipment?> Handle(GetEquipmentByIdQuery query)
    {
        return await equipmentRepository.FindByIdAsync(query.EquipmentId);
    }

    public async Task<IEnumerable<Equipment>> Handle(GetAllEquipmentsQuery query)
    {
        return await equipmentRepository.ListAsync();
    }

    public async Task<IEnumerable<Equipment>> Handle(GetEquipmentsByOwnerIdQuery query)
    {
        return await equipmentRepository.FindByOwnerIdAsync(query.OwnerId);
    }

    public async Task<IEnumerable<Equipment>> Handle(GetEquipmentsByTypeQuery query)
    {
        return await equipmentRepository.FindByTypeAsync(query.EquipmentType);
    }

    public async Task<IEnumerable<Equipment>> Handle(GetEquipmentsByStatusQuery query)
    {
        return await equipmentRepository.FindByStatusAsync(query.Status);
    }
}
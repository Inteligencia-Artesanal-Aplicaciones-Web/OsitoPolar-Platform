using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Queries;

namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Services;

/// <summary>
/// Defines the contract for query-based operations on Equipment.
/// </summary>
public interface IEquipmentQueryService
{
    Task<Equipment?> Handle(GetEquipmentByIdQuery query);
    Task<IEnumerable<Equipment>> Handle(GetAllEquipmentsQuery query);
    Task<IEnumerable<Equipment>> Handle(GetEquipmentsByOwnerIdQuery query);
    Task<IEnumerable<Equipment>> Handle(GetEquipmentsByTypeQuery query);
    Task<IEnumerable<Equipment>> Handle(GetEquipmentsByStatusQuery query);
}
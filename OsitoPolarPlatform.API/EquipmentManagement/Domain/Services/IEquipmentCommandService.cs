using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Commands;

namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Services;

/// <summary>
/// Defines the contract for command-based operations on Equipment.
/// </summary>
public interface IEquipmentCommandService
{
    Task<Equipment?> Handle(CreateEquipmentCommand command);
    Task<Equipment?> Handle(UpdateEquipmentTemperatureCommand command);
    Task<Equipment?> Handle(UpdateEquipmentPowerStateCommand command);
    Task<Equipment?> Handle(UpdateEquipmentLocationCommand command);
    Task<bool> Handle(DeleteEquipmentCommand command);
}
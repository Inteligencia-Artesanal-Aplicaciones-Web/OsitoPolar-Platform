using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Commands;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Repositories;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Services;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;

namespace OsitoPolarPlatform.API.EquipmentManagement.Application.Internal.CommandServices;

/// <summary>
/// Concrete implementation of IEquipmentCommandService.
/// </summary>
public class EquipmentCommandService(
    IEquipmentRepository equipmentRepository,
    IUnitOfWork unitOfWork) : IEquipmentCommandService
{
    public async Task<Equipment?> Handle(CreateEquipmentCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ArgumentException("Equipment name is required.");
        if (string.IsNullOrWhiteSpace(command.SerialNumber))
            throw new ArgumentException("Serial number is required.");
        if (string.IsNullOrWhiteSpace(command.Code))
            throw new ArgumentException("Equipment code is required.");

        if (await equipmentRepository.ExistsBySerialNumberAsync(command.SerialNumber))
            throw new InvalidOperationException($"Equipment with serial number {command.SerialNumber} already exists.");
        if (await equipmentRepository.ExistsByCodeAsync(command.Code))
            throw new InvalidOperationException($"Equipment with code {command.Code} already exists.");

        var equipment = new Equipment(command);
        await equipmentRepository.AddAsync(equipment);
        await unitOfWork.CompleteAsync();

        return equipment;
    }

    public async Task<Equipment?> Handle(UpdateEquipmentTemperatureCommand command)
    {
        var equipment = await equipmentRepository.FindByIdAsync(command.EquipmentId);
        if (equipment is null) return null;

        equipment.Handle(command);
        await unitOfWork.CompleteAsync();
        
        return equipment;
    }

    public async Task<Equipment?> Handle(UpdateEquipmentPowerStateCommand command)
    {
        var equipment = await equipmentRepository.FindByIdAsync(command.EquipmentId);
        if (equipment is null) return null;

        equipment.Handle(command);
        await unitOfWork.CompleteAsync();
        
        return equipment;
    }

    public async Task<Equipment?> Handle(UpdateEquipmentLocationCommand command)
    {
        var equipment = await equipmentRepository.FindByIdAsync(command.EquipmentId);
        if (equipment is null) return null;

        equipment.Handle(command);
        await unitOfWork.CompleteAsync();
        
        return equipment;
    }
    public async Task<bool> Handle(DeleteEquipmentCommand command)
    {
        var equipment = await equipmentRepository.FindByIdAsync(command.EquipmentId);
        if (equipment is null) return false;

        equipmentRepository.Remove(equipment);
        await unitOfWork.CompleteAsync();
        return true;
    }
}
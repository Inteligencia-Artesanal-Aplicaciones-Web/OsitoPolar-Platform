using OsitoPolarPlatform.API.Analytics.Application.Internal.Services;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Commands;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Repositories;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Services;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace OsitoPolarPlatform.API.EquipmentManagement.Application.Internal.CommandServices;

/// <summary>
/// Concrete implementation of IEquipmentCommandService.
/// </summary>
public class EquipmentCommandService : IEquipmentCommandService  
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAutomaticDataGeneratorService _dataGenerator;
    private readonly ILogger<EquipmentCommandService> _logger;

    public EquipmentCommandService(
        IEquipmentRepository equipmentRepository,
        IUnitOfWork unitOfWork,
        IAutomaticDataGeneratorService dataGenerator,
        ILogger<EquipmentCommandService> logger)
    {
        _equipmentRepository = equipmentRepository;
        _unitOfWork = unitOfWork;
        _dataGenerator = dataGenerator;
        _logger = logger;
    }

    public async Task<Equipment?> Handle(CreateEquipmentCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ArgumentException("Equipment name is required.");
        if (string.IsNullOrWhiteSpace(command.SerialNumber))
            throw new ArgumentException("Serial number is required.");
        if (string.IsNullOrWhiteSpace(command.Code))
            throw new ArgumentException("Equipment code is required.");

        if (await _equipmentRepository.ExistsBySerialNumberAsync(command.SerialNumber))
            throw new InvalidOperationException($"Equipment with serial number {command.SerialNumber} already exists.");
        if (await _equipmentRepository.ExistsByCodeAsync(command.Code))
            throw new InvalidOperationException($"Equipment with code {command.Code} already exists.");

        var equipment = new Equipment(command);
        await _equipmentRepository.AddAsync(equipment);
        await _unitOfWork.CompleteAsync();

        // GENERAR DATOS AUTOMÁTICAMENTE DESPUÉS DE CREAR EL EQUIPO
        try
        {
            _logger.LogInformation($"Generating initial analytics data for equipment {equipment.Id}...");
            await _dataGenerator.GenerateInitialDataForEquipmentAsync(equipment.Id);
            _logger.LogInformation($"Analytics data generated successfully for equipment {equipment.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to generate analytics data for equipment {equipment.Id}. Data can be generated on first access.");
        }

        return equipment;
    }

    public async Task<Equipment?> Handle(UpdateEquipmentTemperatureCommand command)
    {
        var equipment = await _equipmentRepository.FindByIdAsync(command.EquipmentId);
        if (equipment is null) return null;

        equipment.Handle(command);
        await _unitOfWork.CompleteAsync();
        
        return equipment;
    }

    public async Task<Equipment?> Handle(UpdateEquipmentPowerStateCommand command)
    {
        var equipment = await _equipmentRepository.FindByIdAsync(command.EquipmentId);
        if (equipment is null) return null;

        equipment.Handle(command);
        await _unitOfWork.CompleteAsync();
        
        return equipment;
    }

    public async Task<Equipment?> Handle(UpdateEquipmentLocationCommand command)
    {
        var equipment = await _equipmentRepository.FindByIdAsync(command.EquipmentId);
        if (equipment is null) return null;

        equipment.Handle(command);
        await _unitOfWork.CompleteAsync();
        
        return equipment;
    }
    
    public async Task<bool> Handle(DeleteEquipmentCommand command)
    {
        var equipment = await _equipmentRepository.FindByIdAsync(command.EquipmentId);
        if (equipment is null) return false;

        _equipmentRepository.Remove(equipment);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
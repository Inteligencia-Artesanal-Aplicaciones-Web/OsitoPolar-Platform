namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Commands;

public record UpdateEquipmentTemperatureCommand(int EquipmentId, decimal NewTemperature);
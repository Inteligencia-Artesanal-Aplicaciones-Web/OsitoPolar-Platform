namespace OsitoPolarPlatform.API.Analytics.Domain.Model.Commands;

public record RecordEnergyReadingCommand(
    int EquipmentId,
    decimal Consumption,
    string Unit = "watts"
);
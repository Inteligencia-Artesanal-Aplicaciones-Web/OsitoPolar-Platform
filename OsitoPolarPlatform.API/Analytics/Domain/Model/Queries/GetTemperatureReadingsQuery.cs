namespace OsitoPolarPlatform.API.Analytics.Domain.Model.Queries;

public record GetTemperatureReadingsQuery(int EquipmentId, int Hours = 24);
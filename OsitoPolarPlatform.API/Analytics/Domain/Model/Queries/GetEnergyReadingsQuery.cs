namespace OsitoPolarPlatform.API.Analytics.Domain.Model.Queries;

public record GetEnergyReadingsQuery(int EquipmentId, int Hours = 24);
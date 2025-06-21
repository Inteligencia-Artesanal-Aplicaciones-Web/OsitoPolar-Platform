namespace OsitoPolarPlatform.API.Analytics.Domain.Model.Queries;

public record GetDailyTemperatureAveragesQuery(int EquipmentId, int Days = 7);
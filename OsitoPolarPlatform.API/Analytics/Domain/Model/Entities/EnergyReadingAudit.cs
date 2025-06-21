using System.ComponentModel.DataAnnotations.Schema;
using EntityFrameworkCore.CreatedUpdatedDate.Contracts;

namespace OsitoPolarPlatform.API.Analytics.Domain.Model.Entities;

public partial class EnergyReading : IEntityWithCreatedUpdatedDate
{
    [Column("CreatedAt")] public DateTimeOffset? CreatedDate { get; set; }
    [Column("UpdatedAt")] public DateTimeOffset? UpdatedDate { get; set; }
}
namespace OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Aggregates;
using System.ComponentModel.DataAnnotations.Schema;
using EntityFrameworkCore.CreatedUpdatedDate.Contracts; // Asegúrate de tener este paquete o su implementación


public partial class ServiceRequest : IEntityWithCreatedUpdatedDate
{
    [Column("CreatedAt")] public DateTimeOffset? CreatedDate { get; set; }
    [Column("UpdatedAt")] public DateTimeOffset? UpdatedDate { get; set; }
}
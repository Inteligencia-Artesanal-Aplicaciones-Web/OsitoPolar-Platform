using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Entities;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Queries;

namespace OsitoPolarPlatform.API.WorkOrders.Domain.Services;

public interface ITechnicianQueryService
{
    Task<Technician?> Handle(GetTechnicianByIdQuery query);
    Task<IEnumerable<Technician>> Handle(GetAllTechniciansQuery query);
    Task<double> Handle(GetTechnicianAverageRatingQuery query);
}

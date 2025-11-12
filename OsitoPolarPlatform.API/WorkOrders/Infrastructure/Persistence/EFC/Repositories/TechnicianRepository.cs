using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Entities;
using OsitoPolarPlatform.API.WorkOrders.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace OsitoPolarPlatform.API.WorkOrders.Infrastructure.Persistence.EFC.Repositories;

public class TechnicianRepository(AppDbContext context) :
    BaseRepository<Technician>(context),
    ITechnicianRepository;

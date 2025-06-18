using OsitoPolarPlatform.API.bc_technicians.Domain.Model.Entities;
using OsitoPolarPlatform.API.bc_technicians.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace OsitoPolarPlatform.API.bc_technicians.Infrastructure.Persistence.EFC.Repositories;

public class TechnicianRepository(AppDbContext context) : 
    BaseRepository<Technician>(context), 
    ITechnicianRepository;


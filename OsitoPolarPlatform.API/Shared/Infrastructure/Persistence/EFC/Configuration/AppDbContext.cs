using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using OsitoPolarPlatform.API.bc_technicians.Domain.Model.Entities;
using OsitoPolarPlatform.API.ServiceRequests.Infrastructure.Persistence.EFC.Configuration.Extensions;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using OsitoPolarPlatform.API.WorkOrders.Infrastructure.Persistence.EFC.Configuration.Extensions;

namespace OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Technician> Technicians { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        // Add the created and updated interceptor
        builder.AddCreatedUpdatedInterceptor();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Apply snake_case naming convention for database
        builder.ApplyServiceRequestConfiguration();
        
        //technicians
        builder.Entity<Technician>().HasKey(f => f.Id);
        builder.Entity<Technician>().Property(f => f.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Technician>().Property(f => f.Name).IsRequired();
        builder.Entity<Technician>().Property(f => f.CompanyId).IsRequired();
        builder.Entity<Technician>().Property(f => f.Specialization).IsRequired();
        builder.Entity<Technician>().Property(f => f.Email).IsRequired();
        builder.Entity<Technician>().Property(f => f.Phone).IsRequired();
        builder.Entity<Technician>().Property(f => f.Rating).IsRequired();
        builder.Entity<Technician>().Property(f => f.Availability).IsRequired();

        
        
        builder.ApplyWorkOrderConfiguration();
        builder.UseSnakeCaseNamingConvention();
    }
}
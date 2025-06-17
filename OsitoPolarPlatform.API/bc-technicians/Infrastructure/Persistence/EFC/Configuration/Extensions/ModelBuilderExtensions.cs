using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.bc_technicians.Domain.Model.Entities;

namespace OsitoPolarPlatform.API.bc_technicians.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
/// Extensions for configuring the Entity Framework Core model builder for the Technicians context.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies the configuration for the Technicians context entities.
    /// </summary>
    public static void ApplyTechnicianConfiguration(this ModelBuilder builder)
    {
        builder.Entity<Technician>().HasKey(t => t.Id);
        builder.Entity<Technician>().Property(t => t.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Technician>().Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Entity<Technician>().Property(t => t.CompanyId).IsRequired();
        builder.Entity<Technician>().Property(t => t.Specialization).IsRequired().HasMaxLength(100);
        builder.Entity<Technician>().Property(t => t.Email).IsRequired().HasMaxLength(100);
        builder.Entity<Technician>().Property(t => t.Phone).IsRequired().HasMaxLength(20);
        builder.Entity<Technician>().Property(t => t.Rating).IsRequired().HasColumnType("decimal(3,2)");
        builder.Entity<Technician>().Property(t => t.Availability).IsRequired().HasMaxLength(50);
    }
}
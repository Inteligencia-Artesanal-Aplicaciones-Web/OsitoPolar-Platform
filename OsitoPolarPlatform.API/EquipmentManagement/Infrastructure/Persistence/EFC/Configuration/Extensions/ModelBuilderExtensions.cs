using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Aggregates;

namespace OsitoPolarPlatform.API.EquipmentManagement.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
/// Extensions for configuring the Entity Framework Core model builder for the Equipment context.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies the configuration for the Equipment context entities.
    /// </summary>
    public static void ApplyEquipmentConfiguration(this ModelBuilder builder)
    {
        // Equipment Entity Configuration
        builder.Entity<Equipment>(entity =>
        {
            // Primary Key
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                  .HasColumnName("equipment_id")
                  .IsRequired()
                  .ValueGeneratedOnAdd();

            // Basic Properties
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SerialNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.SerialNumber).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Model).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Manufacturer).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TechnicalDetails).HasMaxLength(2000);
            entity.Property(e => e.OwnerType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            // Enum Properties
            entity.Property(e => e.Type).IsRequired().HasConversion<string>();
            entity.Property(e => e.Status).IsRequired().HasConversion<string>();
            entity.Property(e => e.OwnershipType).IsRequired().HasConversion<string>();

            // Decimal Properties
            entity.Property(e => e.Cost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CurrentTemperature).HasColumnType("decimal(5,2)");
            entity.Property(e => e.SetTemperature).HasColumnType("decimal(5,2)");
            entity.Property(e => e.OptimalTemperatureMin).HasColumnType("decimal(5,2)");
            entity.Property(e => e.OptimalTemperatureMax).HasColumnType("decimal(5,2)");

            // EquipmentIdentifier Value Object
            entity.OwnsOne(e => e.EquipmentIdentifier, ei =>
            {
                ei.Property(p => p.Identifier).HasColumnName("equipment_identifier");
            });
// Location Value Object
            entity.OwnsOne(e => e.Location, l =>
            {
                l.Property(p => p.Name).HasColumnName("location_name").HasMaxLength(100);
                l.Property(p => p.Address).HasColumnName("location_address").HasMaxLength(255);

                l.OwnsOne(p => p.Coordinates, c =>
                {
                    c.Property(p => p.Latitude).HasColumnName("location_latitude").HasColumnType("decimal(10,8)");
                    c.Property(p => p.Longitude).HasColumnName("location_longitude").HasColumnType("decimal(11,8)");
                    
                    c.WithOwner().HasForeignKey("equipment_id");
                });
            });



            // EnergyConsumption Value Object
            entity.OwnsOne(e => e.EnergyConsumption, ec =>
            {
                ec.Property(p => p.Current).HasColumnName("energy_consumption_current").HasColumnType("decimal(10,2)");
                ec.Property(p => p.Unit).HasColumnName("energy_consumption_unit").HasMaxLength(20);
                ec.Property(p => p.Average).HasColumnName("energy_consumption_average").HasColumnType("decimal(10,2)");
            });

            // RentalInfo Value Object (nullable)
            entity.OwnsOne(e => e.RentalInfo, ri =>
            {
                ri.Property(p => p.StartDate).HasColumnName("rental_start_date");
                ri.Property(p => p.EndDate).HasColumnName("rental_end_date");
                ri.Property(p => p.MonthlyFee).HasColumnName("rental_monthly_fee").HasColumnType("decimal(10,2)");
                ri.Property(p => p.ProviderId).HasColumnName("rental_provider_id");
            });

            // Audit Properties
            entity.Property(e => e.CreatedDate).HasColumnName("created_at");
            entity.Property(e => e.UpdatedDate).HasColumnName("updated_at");
        });
    }
}

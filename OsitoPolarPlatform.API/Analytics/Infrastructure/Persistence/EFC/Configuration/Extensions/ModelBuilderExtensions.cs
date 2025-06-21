using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.Analytics.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Analytics.Domain.Model.Entities;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Aggregates;

namespace OsitoPolarPlatform.API.Analytics.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
/// Extensions for configuring Analytics context entities
/// </summary>
public static class ModelBuilderExtensions
{
    public static void ApplyAnalyticsConfiguration(this ModelBuilder builder)
    {
        // TemperatureReading Configuration
        builder.Entity<TemperatureReading>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(t => t.EquipmentId).IsRequired();
            entity.Property(t => t.Temperature).HasColumnType("decimal(5,2)").IsRequired();
            entity.Property(t => t.Status).IsRequired().HasConversion<string>();
            entity.Property(t => t.Timestamp).IsRequired();
            
            entity.HasIndex(t => new { t.EquipmentId, t.Timestamp });
            entity.Property(t => t.CreatedDate).HasColumnName("created_at");
            entity.Property(t => t.UpdatedDate).HasColumnName("updated_at");
            
            entity.HasOne<Equipment>()
                  .WithMany()
                  .HasForeignKey(t => t.EquipmentId)
                  .HasPrincipalKey(e => e.Id)
                  .HasConstraintName("FK_TempReading_Equipment")
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.ToTable("temperature_readings");
        });

        // EnergyReading Configuration
        builder.Entity<EnergyReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.EquipmentId).IsRequired();
            entity.Property(e => e.Consumption).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.Unit).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasConversion<string>();
            entity.Property(e => e.Timestamp).IsRequired();
            
            entity.HasIndex(e => new { e.EquipmentId, e.Timestamp });
            entity.Property(e => e.CreatedDate).HasColumnName("created_at");
            entity.Property(e => e.UpdatedDate).HasColumnName("updated_at");
            
            entity.HasOne<Equipment>()
                  .WithMany()
                  .HasForeignKey(e => e.EquipmentId)
                  .HasPrincipalKey(eq => eq.Id)
                  .HasConstraintName("FK_EnergyReading_Equipment")
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.ToTable("energy_readings");
        });

        // DailyTemperatureAverage Configuration
        builder.Entity<DailyTemperatureAverage>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(d => d.EquipmentId).IsRequired();
            entity.Property(d => d.Date).IsRequired();
            entity.Property(d => d.AverageTemperature).HasColumnType("decimal(5,2)").IsRequired();
            entity.Property(d => d.MinTemperature).HasColumnType("decimal(5,2)").IsRequired();
            entity.Property(d => d.MaxTemperature).HasColumnType("decimal(5,2)").IsRequired();
            
            entity.HasIndex(d => new { d.EquipmentId, d.Date }).IsUnique();
            
            entity.HasOne<Equipment>()
                  .WithMany()
                  .HasForeignKey(d => d.EquipmentId)
                  .HasPrincipalKey(e => e.Id)
                  .HasConstraintName("FK_DailyAvg_Equipment")
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.ToTable("daily_temperature_averages");
        });

        // EquipmentAnalytics Configuration
        builder.Entity<EquipmentAnalytics>(entity =>
        {
            entity.HasKey(ea => ea.Id);
            entity.Property(ea => ea.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(ea => ea.EquipmentId).IsRequired();
    
            entity.HasIndex(ea => ea.EquipmentId).IsUnique();
            entity.Property(ea => ea.CreatedDate).HasColumnName("created_at");
            entity.Property(ea => ea.UpdatedDate).HasColumnName("updated_at");
            
            entity.Ignore(ea => ea.TemperatureReadings);
            entity.Ignore(ea => ea.EnergyReadings);
            
            entity.HasOne<Equipment>()
                .WithMany()
                .HasForeignKey(ea => ea.EquipmentId)
                .HasPrincipalKey(e => e.Id)
                .HasConstraintName("FK_Analytics_Equipment")
                .OnDelete(DeleteBehavior.Cascade);
    
            entity.ToTable("equipment_analytics");
        });
    }
}
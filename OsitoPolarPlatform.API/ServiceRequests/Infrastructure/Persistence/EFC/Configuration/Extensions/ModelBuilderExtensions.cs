using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Aggregates;

namespace OsitoPolarPlatform.API.ServiceRequests.Infrastructure.Persistence.EFC.Configuration.Extensions;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Extensions for configuring the Entity Framework Core model builder for the Service Request context.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies the configuration for the Service Request context entities.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="ModelBuilder"/> instance to apply the configuration to.
    /// </param>
    public static void ApplyServiceRequestConfiguration(this ModelBuilder builder)
    {
        // ServiceRequest Entity Configuration
        builder.Entity<ServiceRequest>().HasKey(sr => sr.Id);
        builder.Entity<ServiceRequest>().Property(sr => sr.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<ServiceRequest>().Property(sr => sr.OrderNumber).IsRequired().HasMaxLength(50);
        builder.Entity<ServiceRequest>().Property(sr => sr.Title).IsRequired().HasMaxLength(100);
        builder.Entity<ServiceRequest>().Property(sr => sr.Description).IsRequired().HasMaxLength(1000);
        builder.Entity<ServiceRequest>().Property(sr => sr.IssueDetails).IsRequired().HasMaxLength(2000);
        builder.Entity<ServiceRequest>().Property(sr => sr.RequestTime).IsRequired();
        builder.Entity<ServiceRequest>().Property(sr => sr.Status).IsRequired().HasConversion<string>(); 
        builder.Entity<ServiceRequest>().Property(sr => sr.Priority).IsRequired().HasConversion<string>(); 
        builder.Entity<ServiceRequest>().Property(sr => sr.Urgency).IsRequired().HasConversion<string>(); 
        builder.Entity<ServiceRequest>().Property(sr => sr.IsEmergency).IsRequired();
        builder.Entity<ServiceRequest>().Property(sr => sr.ServiceType).IsRequired().HasConversion<string>(); 
        
        builder.Entity<ServiceRequest>().Property(sr => sr.ScheduledDate); 
        builder.Entity<ServiceRequest>().Property(sr => sr.TimeSlot).HasMaxLength(50);
        builder.Entity<ServiceRequest>().Property(sr => sr.ServiceAddress).HasMaxLength(255);
        
        builder.Entity<ServiceRequest>().Property(sr => sr.DesiredCompletionDate);
        builder.Entity<ServiceRequest>().Property(sr => sr.ActualCompletionDate);
        builder.Entity<ServiceRequest>().Property(sr => sr.ResolutionDetails).HasMaxLength(2000);
        builder.Entity<ServiceRequest>().Property(sr => sr.TechnicianNotes).HasMaxLength(2000);
        builder.Entity<ServiceRequest>().Property(sr => sr.Cost).HasColumnType("decimal(18,2)"); 
        builder.Entity<ServiceRequest>().Property(sr => sr.CustomerFeedbackRating);

       // builder.Entity<ServiceRequest>()
       //     .HasOne<Equipment.Domain.Model.Entities.Equipment>(sr => sr.Equipment)
       //     .WithMany() 
       //     .HasForeignKey(sr => sr.EquipmentId)
       //     .IsRequired()
       //     .OnDelete(DeleteBehavior.Cascade); 

        //builder.Entity<ServiceRequest>()
        //    .HasOne<User>(sr => sr.ReportedByUser)
        //    .WithMany()
        //    .HasForeignKey(sr => sr.ReportedByUserId)
        //    .IsRequired(false) 
        //    .OnDelete(DeleteBehavior.Restrict);

        //builder.Entity<ServiceRequest>()
        //    .HasOne<Technician>(sr => sr.AssignedTechnician)
        //    .WithMany()
        //    .HasForeignKey(sr => sr.AssignedTechnicianId)
        //    .IsRequired(false) 
        //    .OnDelete(DeleteBehavior.Restrict);

        
         builder.Entity<ServiceRequest>().Property(sr => sr.CreatedDate).HasColumnName("CreatedAt");
         builder.Entity<ServiceRequest>().Property(sr => sr.UpdatedDate).HasColumnName("UpdatedAt");
    }
}
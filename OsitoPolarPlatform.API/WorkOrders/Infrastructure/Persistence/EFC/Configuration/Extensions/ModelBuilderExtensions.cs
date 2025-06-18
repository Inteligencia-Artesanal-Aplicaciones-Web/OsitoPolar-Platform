using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Aggregates; 
using OsitoPolarPlatform.API.bc_technicians.Domain.Model.Entities; 
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Aggregates; 


namespace OsitoPolarPlatform.API.WorkOrders.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
/// Extensions for configuring the Entity Framework Core model builder for the Work Order context.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies the configuration for the Work Order context entities.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="ModelBuilder"/> instance to apply the configuration to.
    /// </param>
    public static void ApplyWorkOrderConfiguration(this ModelBuilder builder)
    {
        
        builder.Entity<WorkOrder>().HasKey(wo => wo.Id);
        builder.Entity<WorkOrder>().Property(wo => wo.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<WorkOrder>().Property(wo => wo.WorkOrderNumber).IsRequired().HasMaxLength(50);
        builder.Entity<WorkOrder>().HasIndex(wo => wo.WorkOrderNumber).IsUnique(); 
        builder.Entity<WorkOrder>().Property(wo => wo.ServiceRequestId); 
        builder.Entity<WorkOrder>().Property(wo => wo.Title).IsRequired().HasMaxLength(100);
        builder.Entity<WorkOrder>().Property(wo => wo.Description).IsRequired().HasMaxLength(1000);
        builder.Entity<WorkOrder>().Property(wo => wo.IssueDetails).IsRequired().HasMaxLength(2000);
        builder.Entity<WorkOrder>().Property(wo => wo.CreationTime).IsRequired();
        builder.Entity<WorkOrder>().Property(wo => wo.Status).IsRequired().HasConversion<string>(); 
       // builder.Entity<WorkOrder>().Property(wo => wo.EquipmentId).IsRequired();
       // builder.Entity<WorkOrder>().Property(wo => wo.ServiceType).IsRequired().HasConversion<string>();
        builder.Entity<WorkOrder>().Property(wo => wo.Priority).IsRequired().HasConversion<string>(); 
        builder.Entity<WorkOrder>().Property(wo => wo.AssignedTechnicianId); 
        builder.Entity<WorkOrder>().Property(wo => wo.ScheduledDate); 
        builder.Entity<WorkOrder>().Property(wo => wo.TimeSlot).HasMaxLength(50);
        builder.Entity<WorkOrder>().Property(wo => wo.ServiceAddress).HasMaxLength(255);
        builder.Entity<WorkOrder>().Property(wo => wo.DesiredCompletionDate); 
        builder.Entity<WorkOrder>().Property(wo => wo.ActualCompletionDate); 

        builder.Entity<WorkOrder>().Property(wo => wo.ResolutionDetails).HasMaxLength(2000); 
        builder.Entity<WorkOrder>().Property(wo => wo.TechnicianNotes).HasMaxLength(2000); 
        builder.Entity<WorkOrder>().Property(wo => wo.Cost).HasColumnType("decimal(18,2)"); 

        builder.Entity<WorkOrder>().Property(wo => wo.CustomerFeedbackRating); 
        builder.Entity<WorkOrder>().Property(wo => wo.FeedbackSubmissionDate); 

        builder.Entity<WorkOrder>().Property(wo => wo.CreatedDate).HasColumnName("CreatedAt");
        builder.Entity<WorkOrder>().Property(wo => wo.UpdatedDate).HasColumnName("UpdatedAt");

        builder.Entity<WorkOrder>()
            .HasOne<ServiceRequest>() 
            .WithMany() 
            .HasForeignKey(wo => wo.ServiceRequestId) 
            .IsRequired(false) 
            .OnDelete(DeleteBehavior.SetNull); 
        
        builder.Entity<WorkOrder>()
            .HasOne<Technician>() 
            .WithMany() 
            .HasForeignKey(wo => wo.AssignedTechnicianId) 
            .IsRequired(false) 
            .OnDelete(DeleteBehavior.SetNull); 
        
        builder.Entity<WorkOrder>()
            .HasOne<Equipment>() 
            .WithMany() 
            .HasForeignKey(wo => wo.EquipmentId) 
            .IsRequired(); 
            
                                                  
    }
}
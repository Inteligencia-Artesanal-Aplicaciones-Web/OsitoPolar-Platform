using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.Notifications.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Notifications.Domain.Model.Entities;
using OsitoPolarPlatform.API.Notifications.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.Notifications.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
/// EF Core configuration extensions for Notifications bounded context
/// </summary>
public static class NotificationsConfigurationExtensions
{
    public static void ApplyNotificationsConfiguration(this ModelBuilder modelBuilder)
    {
        // NotificationLog entity configuration
        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.ToTable("notifications_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasConversion(
                    v => v.ToString(),
                    v => (NotificationType)Enum.Parse(typeof(NotificationType), v))
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Recipient)
                .HasColumnName("recipient")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Template)
                .HasColumnName("template")
                .HasConversion(
                    v => v.HasValue ? v.Value.ToString() : null,
                    v => !string.IsNullOrEmpty(v) ? (EmailTemplate)Enum.Parse(typeof(EmailTemplate), v) : (EmailTemplate?)null)
                .HasMaxLength(50);

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion(
                    v => v.ToString(),
                    v => (NotificationStatus)Enum.Parse(typeof(NotificationStatus), v))
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.ErrorMessage)
                .HasColumnName("error_message")
                .HasColumnType("TEXT");

            entity.Property(e => e.Metadata)
                .HasColumnName("metadata")
                .HasColumnType("TEXT");

            entity.Property(e => e.SentAt)
                .HasColumnName("sent_at");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            // Indexes
            entity.HasIndex(e => e.Recipient).HasDatabaseName("idx_recipient");
            entity.HasIndex(e => e.Status).HasDatabaseName("idx_status");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("idx_created_at");
        });

        // InAppNotification entity configuration
        modelBuilder.Entity<InAppNotification>(entity =>
        {
            entity.ToTable("in_app_notifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Message)
                .HasColumnName("message")
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Severity)
                .HasColumnName("severity")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.EquipmentId)
                .HasColumnName("equipment_id");

            entity.Property(e => e.ServiceRequestId)
                .HasColumnName("service_request_id");

            entity.Property(e => e.IsRead)
                .HasColumnName("is_read")
                .IsRequired();

            entity.Property(e => e.Timestamp)
                .HasColumnName("timestamp")
                .IsRequired();

            entity.Property(e => e.ReadAt)
                .HasColumnName("read_at");

            entity.Property(e => e.Metadata)
                .HasColumnName("metadata")
                .HasColumnType("TEXT");

            // Indexes for performance
            entity.HasIndex(e => e.UserId).HasDatabaseName("idx_user_id");
            entity.HasIndex(e => new { e.UserId, e.IsRead }).HasDatabaseName("idx_user_id_is_read");
            entity.HasIndex(e => e.Timestamp).HasDatabaseName("idx_timestamp");
            entity.HasIndex(e => e.EquipmentId).HasDatabaseName("idx_equipment_id");
            entity.HasIndex(e => e.ServiceRequestId).HasDatabaseName("idx_service_request_id");
        });
    }
}

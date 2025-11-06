using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.Profiles.Domain.Model.Aggregates;

namespace OsitoPolarPlatform.API.Profiles.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyProfilesConfiguration(this ModelBuilder builder)
    {
        // Profiles Context - Base Profile entity

        builder.Entity<Profile>().HasKey(p => p.Id);
        builder.Entity<Profile>().Property(p => p.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Profile>().OwnsOne(p => p.Name,
            n =>
            {
                n.WithOwner().HasForeignKey("Id");
                n.Property(p => p.FirstName).HasColumnName("FirstName");
                n.Property(p => p.LastName).HasColumnName("LastName");
            });

        builder.Entity<Profile>().OwnsOne(p => p.Email,
            e =>
            {
                e.WithOwner().HasForeignKey("Id");
                e.Property(a => a.Address).HasColumnName("EmailAddress");
            });

        builder.Entity<Profile>().OwnsOne(p => p.Address,
            a =>
            {
                a.WithOwner().HasForeignKey("Id");
                a.Property(s => s.Street).HasColumnName("AddressStreet");
                a.Property(s => s.Number).HasColumnName("AddressNumber");
                a.Property(s => s.City).HasColumnName("AddressCity");
                a.Property(s => s.PostalCode).HasColumnName("AddressPostalCode");
                a.Property(s => s.Country).HasColumnName("AddressCountry");
            });

        // Owner entity configuration (Table-per-Type inheritance)
        builder.Entity<Owner>().ToTable("profiles_owners");
        builder.Entity<Owner>().Property(o => o.UserId).IsRequired();
        builder.Entity<Owner>().Property(o => o.Balance).HasColumnType("decimal(18,2)").IsRequired();
        builder.Entity<Owner>().Property(o => o.PlanId).IsRequired();
        builder.Entity<Owner>().Property(o => o.MaxUnits).IsRequired();
        builder.Entity<Owner>().HasIndex(o => o.UserId).IsUnique();

        // RenterProvider entity configuration (Table-per-Type inheritance)
        builder.Entity<RenterProvider>().ToTable("profiles_renter_providers");
        builder.Entity<RenterProvider>().Property(rp => rp.UserId).IsRequired();
        builder.Entity<RenterProvider>().Property(rp => rp.Balance).HasColumnType("decimal(18,2)").IsRequired();
        builder.Entity<RenterProvider>().Property(rp => rp.PlanId).IsRequired();
        builder.Entity<RenterProvider>().Property(rp => rp.MaxClients).IsRequired();
        builder.Entity<RenterProvider>().Property(rp => rp.CompanyName).HasMaxLength(200).IsRequired();
        builder.Entity<RenterProvider>().Property(rp => rp.TaxId).HasMaxLength(50);
        builder.Entity<RenterProvider>().HasIndex(rp => rp.UserId).IsUnique();
        builder.Entity<RenterProvider>().HasIndex(rp => rp.CompanyName);
    }
}
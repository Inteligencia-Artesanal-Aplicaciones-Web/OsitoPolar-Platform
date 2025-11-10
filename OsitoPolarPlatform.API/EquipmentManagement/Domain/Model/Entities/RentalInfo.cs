using Microsoft.EntityFrameworkCore;

namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Entities;

[Owned]
public class RentalInfo
{
    public DateTimeOffset? StartDate { get; private set; }
    public DateTimeOffset? EndDate { get; private set; }
    public decimal MonthlyFee { get; private set; }
    public int ProviderId { get; private set; }

    protected RentalInfo() { }

    // Constructor for equipment published for rent (not yet rented - dates are null)
    public RentalInfo(decimal monthlyFee, int providerId)
    {
        if (monthlyFee <= 0)
            throw new ArgumentException("Monthly fee must be positive", nameof(monthlyFee));
        if (providerId <= 0)
            throw new ArgumentException("Provider ID must be positive", nameof(providerId));

        MonthlyFee = monthlyFee;
        ProviderId = providerId;
        StartDate = null;
        EndDate = null;
    }

    // Constructor for equipment that has been rented (with rental dates)
    public RentalInfo(DateTimeOffset startDate, DateTimeOffset endDate, decimal monthlyFee, int providerId)
    {
        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");
        if (monthlyFee <= 0)
            throw new ArgumentException("Monthly fee must be positive", nameof(monthlyFee));
        if (providerId <= 0)
            throw new ArgumentException("Provider ID must be positive", nameof(providerId));

        StartDate = startDate;
        EndDate = endDate;
        MonthlyFee = monthlyFee;
        ProviderId = providerId;
    }

    public bool IsActive()
    {
        if (!StartDate.HasValue || !EndDate.HasValue)
            return false; // Not yet rented, so not active

        var now = DateTimeOffset.UtcNow;
        return now >= StartDate.Value && now <= EndDate.Value;
    }

    public void ExtendRental(DateTimeOffset newEndDate)
    {
        if (!EndDate.HasValue)
            throw new InvalidOperationException("Cannot extend rental that hasn't been rented yet");

        if (newEndDate <= EndDate.Value)
            throw new ArgumentException("New end date must be after current end date");

        EndDate = newEndDate;
    }

    public void SetRentalDates(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");

        StartDate = startDate;
        EndDate = endDate;
    }
}
using Microsoft.EntityFrameworkCore;

namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Entities;

[Owned]
public class RentalInfo
{
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public decimal MonthlyFee { get; private set; }
    public int ProviderId { get; private set; }

    protected RentalInfo() { }

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
        var now = DateTimeOffset.UtcNow;
        return now >= StartDate && now <= EndDate;
    }

    public void ExtendRental(DateTimeOffset newEndDate)
    {
        if (newEndDate <= EndDate)
            throw new ArgumentException("New end date must be after current end date");
        
        EndDate = newEndDate;
    }
}
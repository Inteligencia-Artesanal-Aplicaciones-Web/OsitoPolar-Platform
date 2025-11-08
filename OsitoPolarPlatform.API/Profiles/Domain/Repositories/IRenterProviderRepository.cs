using OsitoPolarPlatform.API.Profiles.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;

namespace OsitoPolarPlatform.API.Profiles.Domain.Repositories;

/// <summary>
/// RenterProvider repository interface
/// </summary>
public interface IRenterProviderRepository : IBaseRepository<RenterProvider>
{
    /// <summary>
    /// Find RenterProvider by User ID (from IAM)
    /// </summary>
    Task<RenterProvider?> FindByUserIdAsync(int userId);

    /// <summary>
    /// Find RenterProvider by email
    /// </summary>
    Task<RenterProvider?> FindByEmailAsync(string email);

    /// <summary>
    /// Find RenterProvider by company name
    /// </summary>
    Task<RenterProvider?> FindByCompanyNameAsync(string companyName);

    /// <summary>
    /// Check if RenterProvider exists by User ID
    /// </summary>
    Task<bool> ExistsByUserIdAsync(int userId);
}

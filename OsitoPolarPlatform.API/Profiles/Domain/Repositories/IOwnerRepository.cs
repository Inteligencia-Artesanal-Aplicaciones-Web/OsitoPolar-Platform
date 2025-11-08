using OsitoPolarPlatform.API.Profiles.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;

namespace OsitoPolarPlatform.API.Profiles.Domain.Repositories;

/// <summary>
/// Owner repository interface
/// </summary>
public interface IOwnerRepository : IBaseRepository<Owner>
{
    /// <summary>
    /// Find Owner by User ID (from IAM)
    /// </summary>
    Task<Owner?> FindByUserIdAsync(int userId);

    /// <summary>
    /// Find Owner by email
    /// </summary>
    Task<Owner?> FindByEmailAsync(string email);

    /// <summary>
    /// Check if Owner exists by User ID
    /// </summary>
    Task<bool> ExistsByUserIdAsync(int userId);
}

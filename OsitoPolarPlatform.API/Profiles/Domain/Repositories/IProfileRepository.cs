using OsitoPolarPlatform.API.Profiles.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Profiles.Domain.Model.ValueObjects;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;

namespace OsitoPolarPlatform.API.Profiles.Domain.Repositories;

public interface IProfileRepository : IBaseRepository<Profile>
{
    Task<Profile?> FindProfileByEmailAsync(EmailAddress email);
}

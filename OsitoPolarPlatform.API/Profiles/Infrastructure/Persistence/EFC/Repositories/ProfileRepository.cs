using OsitoPolarPlatform.API.Profiles.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Profiles.Domain.Model.ValueObjects;
using OsitoPolarPlatform.API.Profiles.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace OsitoPolarPlatform.API.Profiles.Infrastructure.Persistence.EFC.Repositories;

public class ProfileRepository(AppDbContext context)
: BaseRepository<Profile>(context), IProfileRepository
{
 public async Task<Profile?> FindProfileByEmailAsync(EmailAddress email)
 {
  return Context.Set<Profile>().FirstOrDefault(p => p.Email == email);
 }    
}
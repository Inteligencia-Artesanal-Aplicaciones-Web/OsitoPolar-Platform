using OsitoPolarPlatform.API.Profiles.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Profiles.Domain.Model.Commands;

namespace OsitoPolarPlatform.API.Profiles.Domain.Services;

public interface IProfileCommandService
{
    Task<Profile?> Handle(CreateProfileCommand command);
}
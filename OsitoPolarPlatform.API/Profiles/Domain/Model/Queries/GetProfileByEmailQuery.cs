using OsitoPolarPlatform.API.Profiles.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.Profiles.Domain.Model.Queries;

public record GetProfileByEmailQuery(EmailAddress Email);
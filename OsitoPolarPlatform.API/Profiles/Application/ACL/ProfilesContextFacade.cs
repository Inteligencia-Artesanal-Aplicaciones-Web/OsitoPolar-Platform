using OsitoPolarPlatform.API.Profiles.Domain.Model.Commands;
using OsitoPolarPlatform.API.Profiles.Domain.Model.Queries;
using OsitoPolarPlatform.API.Profiles.Domain.Model.ValueObjects;
using OsitoPolarPlatform.API.Profiles.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Profiles.Domain.Services;
using OsitoPolarPlatform.API.Profiles.Domain.Repositories;
using OsitoPolarPlatform.API.Profiles.Interfaces.ACL;

namespace OsitoPolarPlatform.API.Profiles.Application.ACL;

/// <summary>
/// Facade for the profiles context
/// </summary>
/// <param name="profileCommandService">
/// The profile command service
/// </param>
/// <param name="profileQueryService">
/// The profile query service
/// </param>
/// <param name="ownerRepository">
/// The owner repository
/// </param>
/// <param name="renterProviderRepository">
/// The renter provider repository
/// </param>
public class ProfilesContextFacade(
    IProfileCommandService profileCommandService,
    IProfileQueryService profileQueryService,
    IOwnerRepository ownerRepository,
    IRenterProviderRepository renterProviderRepository
    ) : IProfilesContextFacade
{
    
    // inheritedDoc
    public async Task<int> CreateProfile(string firstName, string lastName, string email, string street, string number, string city,
        string postalCode, string country)
    {
        var createProfileCommand = new CreateProfileCommand(firstName, lastName, email, street, number, city, postalCode, country);
        var profile = await profileCommandService.Handle(createProfileCommand);
        return profile?.Id ?? 0;
    }

    // inheritedDoc
    public async Task<int> FetchProfileIdByEmail(string email)
    {
        var getProfileByEmailQuery = new GetProfileByEmailQuery(new EmailAddress(email));
        var profile = await profileQueryService.Handle(getProfileByEmailQuery);
        return profile?.Id ?? 0;
    }

    public async Task<bool> IsUserAnOwner(int userId)
    {
        return await ownerRepository.ExistsByUserIdAsync(userId);
    }

    public async Task<bool> IsUserAProvider(int userId)
    {
        return await renterProviderRepository.ExistsByUserIdAsync(userId);
    }

    public async Task<int> FetchOwnerIdByUserId(int userId)
    {
        var owner = await ownerRepository.FindByUserIdAsync(userId);
        return owner?.Id ?? 0;
    }

    public async Task<int> FetchProviderIdByUserId(int userId)
    {
        var provider = await renterProviderRepository.FindByUserIdAsync(userId);
        return provider?.Id ?? 0;
    }

    public async Task<string> FetchProviderCompanyName(int providerId)
    {
        var provider = await renterProviderRepository.FindByIdAsync(providerId);
        return provider?.CompanyName ?? string.Empty;
    }

    public async Task<(int ownerId, int planId, string firstName, string lastName, string email)?> GetOwnerDataByUserId(int userId)
    {
        var owner = await ownerRepository.FindByUserIdAsync(userId);
        if (owner == null) return null;

        return (owner.Id, owner.PlanId, owner.Name.FirstName, owner.Name.LastName, owner.EmailAddress);
    }

    public async Task<(int providerId, int planId, string companyName, string ruc, string email)?> GetProviderDataByUserId(int userId)
    {
        var provider = await renterProviderRepository.FindByUserIdAsync(userId);
        if (provider == null) return null;

        return (provider.Id, provider.PlanId, provider.CompanyName, provider.CompanyName, provider.EmailAddress);
    }

    public async Task<(int ownerId, decimal balance, int planId, int maxUnits)?> GetOwnerProfileForAuthByUserId(int userId)
    {
        var owner = await ownerRepository.FindByUserIdAsync(userId);
        if (owner == null) return null;

        return (owner.Id, owner.Balance, owner.PlanId, owner.MaxUnits);
    }

    public async Task<(int providerId, decimal balance, int planId, int maxClients, string companyName)?> GetProviderProfileForAuthByUserId(int userId)
    {
        var provider = await renterProviderRepository.FindByUserIdAsync(userId);
        if (provider == null) return null;

        return (provider.Id, provider.Balance, provider.PlanId, provider.MaxClients, provider.CompanyName);
    }

    public async Task<int> CreateOwnerProfile(int userId, string firstName, string lastName, string email,
        string street, string number, string city, string postalCode, string country, int planId, int maxUnits)
    {
        var owner = new Owner(
            userId, firstName, lastName, email, street, number, city, postalCode, country, planId, maxUnits);
        await ownerRepository.AddAsync(owner);
        return owner.Id;
    }

    public async Task<int> CreateProviderProfile(int userId, string companyName, string contactFirstName, string contactLastName,
        string email, string street, string number, string city, string postalCode, string country,
        int planId, int maxClients, string taxId)
    {
        var provider = new RenterProvider(
            userId, companyName, contactFirstName, contactLastName, email, street, number, city, postalCode, country, planId, maxClients, taxId);
        await renterProviderRepository.AddAsync(provider);
        return provider.Id;
    }

    public async Task<int> GetProviderUserIdByProviderId(int providerId)
    {
        var provider = await renterProviderRepository.FindByIdAsync(providerId);
        return provider?.UserId ?? 0;
    }

    public async Task<bool> UpdateProviderBalance(int providerId, decimal amount, string description)
    {
        var provider = await renterProviderRepository.FindByIdAsync(providerId);
        if (provider == null) return false;

        provider.RecordServiceRevenue(amount, description);
        renterProviderRepository.Update(provider);
        return true;
    }

    public async Task<bool> UpdateOwnerPlan(int ownerId, int planId, int maxUnits)
    {
        var owner = await ownerRepository.FindByIdAsync(ownerId);
        if (owner == null) return false;

        owner.UpdatePlan(planId, maxUnits);
        ownerRepository.Update(owner);
        return true;
    }

    public async Task<bool> UpdateProviderPlan(int providerId, int planId, int maxClients)
    {
        var provider = await renterProviderRepository.FindByIdAsync(providerId);
        if (provider == null) return false;

        provider.UpdatePlan(planId, maxClients);
        renterProviderRepository.Update(provider);
        return true;
    }

    public async Task<(string firstName, string lastName)?> GetOwnerNameByOwnerId(int ownerId)
    {
        var owner = await ownerRepository.FindByIdAsync(ownerId);
        if (owner == null) return null;

        return (owner.Name.FirstName, owner.Name.LastName);
    }
}
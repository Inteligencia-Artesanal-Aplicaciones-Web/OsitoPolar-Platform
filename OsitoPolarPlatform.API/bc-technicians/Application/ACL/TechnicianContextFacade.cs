using OsitoPolarPlatform.API.bc_technicians.Domain.Repositories;
using OsitoPolarPlatform.API.bc_technicians.Interfaces.ACL;

namespace OsitoPolarPlatform.API.bc_technicians.Application.ACL;

/// <summary>
/// Facade implementation for the Technicians context
/// </summary>
/// <param name="technicianRepository">The technician repository</param>
public class TechnicianContextFacade(ITechnicianRepository technicianRepository) : ITechnicianContextFacade
{
    public async Task<bool> TechnicianExists(int technicianId)
    {
        var technician = await technicianRepository.FindByIdAsync(technicianId);
        return technician != null;
    }

    public async Task<bool> TechnicianBelongsToCompany(int technicianId, int companyId)
    {
        var technician = await technicianRepository.FindByIdAsync(technicianId);
        if (technician == null) return false;

        return technician.CompanyId == companyId;
    }

    public async Task<string> FetchTechnicianName(int technicianId)
    {
        var technician = await technicianRepository.FindByIdAsync(technicianId);
        return technician?.Name ?? string.Empty;
    }
}

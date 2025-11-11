namespace OsitoPolarPlatform.API.bc_technicians.Interfaces.ACL;

/// <summary>
/// Facade for the Technicians context
/// </summary>
public interface ITechnicianContextFacade
{
    /// <summary>
    /// Check if technician exists
    /// </summary>
    /// <param name="technicianId">Technician ID</param>
    /// <returns>True if technician exists, false otherwise</returns>
    Task<bool> TechnicianExists(int technicianId);

    /// <summary>
    /// Check if technician belongs to a company
    /// </summary>
    /// <param name="technicianId">Technician ID</param>
    /// <param name="companyId">Company ID (Provider ID)</param>
    /// <returns>True if technician belongs to the company, false otherwise</returns>
    Task<bool> TechnicianBelongsToCompany(int technicianId, int companyId);

    /// <summary>
    /// Fetch technician name
    /// </summary>
    /// <param name="technicianId">Technician ID</param>
    /// <returns>Technician name if found, empty string otherwise</returns>
    Task<string> FetchTechnicianName(int technicianId);
}

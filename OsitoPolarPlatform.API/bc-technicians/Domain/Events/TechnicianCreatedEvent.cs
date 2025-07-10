using OsitoPolarPlatform.API.Shared.Domain.Model.Events;

namespace OsitoPolarPlatform.API.bc_technicians.Domain.Events;

public class TechnicianCreatedEvent(string name, string specialization, string phone, string email, string availability, int companyId) 
    : IEvent
{
    public string Name { get; } = name;
    public string Specialization { get; } = specialization;
    public string Phone { get; } = phone;
    public string Email { get; } = email;
    public string Availability { get; } = availability;
    public int CompanyId { get; } = companyId;
}
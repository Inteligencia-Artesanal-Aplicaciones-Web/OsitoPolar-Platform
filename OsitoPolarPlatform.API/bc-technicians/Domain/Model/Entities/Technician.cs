using OsitoPolarPlatform.API.bc_technicians.Domain.Model.Commands;

namespace OsitoPolarPlatform.API.bc_technicians.Domain.Model.Entities;

public class Technician
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Specialization { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public decimal Rating { get; set; }
    public string Availability { get; set; }
    public int CompanyId { get; set; }
    
    public Technician()
    {
        Name = string.Empty;
        Specialization = string.Empty;
        Phone = string.Empty;
        Email = string.Empty;
        Availability = string.Empty;
        CompanyId = 0;
        Rating = 0.0m;
    }

    
    public Technician(string name, string specialization, string phone, string email, string availability, int companyId)
    {
        Name = name;
        Specialization = specialization;
        Phone = phone;
        Email = email;
        Availability = availability;
        CompanyId = companyId;
        Rating = 0.0m;
    }
    
    public Technician(CreateTechnicianCommand command)
        : this(command.Name, command.Specialization, command.Phone, command.Email, command.Availability, command.CompanyId)
    {
    }
    
}
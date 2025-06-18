using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Entities;

/// <summary>
/// Represents the physical location of equipment.
/// </summary>
[Owned]
public class Location
{
    public string Name { get; private set; }
    public string Address { get; private set; }
    public Coordinates Coordinates { get; private set; }

    public Location()
    {
        Name = string.Empty;
        Address = string.Empty;
        Coordinates = new Coordinates(0, 0);
    }

    public Location(string name, string address, decimal latitude, decimal longitude)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Coordinates = new Coordinates(latitude, longitude);
    }

    public void UpdateLocation(string name, string address, decimal latitude, decimal longitude)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Location name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Location address cannot be empty", nameof(address));

        Name = name;
        Address = address;
        Coordinates = new Coordinates(latitude, longitude);
    }
}
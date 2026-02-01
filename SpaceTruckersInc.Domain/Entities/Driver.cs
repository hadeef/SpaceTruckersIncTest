using SpaceTruckersInc.Domain.Common;
using SpaceTruckersInc.Domain.Enums;
using SpaceTruckersInc.Domain.Events;

namespace SpaceTruckersInc.Domain.Entities;

public class Driver : Entity
{
    public Driver(string name, LicenseLevel licenseLevel)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }
        Name = name.Trim();
        LicenseLevel = licenseLevel ?? throw new ArgumentException("LicenseLevel is required.", nameof(licenseLevel));
        Status = DriverStatus.Available;
    }

    public LicenseLevel LicenseLevel { get; private set; }
    public string Name { get; private set; }
    public DriverStatus Status { get; private set; }

    public void ChangeLicense(LicenseLevel newLicense)
    {
        if (newLicense is null)
        {
            throw new ArgumentException("LicenseLevel is required.", nameof(newLicense));
        }

        if (newLicense == LicenseLevel)
        {
            return;
        }

        _ = LicenseLevel;
        LicenseLevel = newLicense;
        DateTime occurredOn = DateTime.UtcNow;
        UpdateTime = occurredOn;
    }

    public void MarkAvailable()
    {
        if (Status == DriverStatus.Available)
        {
            return;
        }

        DriverStatus previous = Status;
        Status = DriverStatus.Available;
        DateTime occurredOn = DateTime.UtcNow;
        UpdateTime = occurredOn;
        RaiseDomainEvent(new DriverStatusChangedEvent(Id, previous, Status, occurredOn));
    }

    public void MarkOnTrip()
    {
        if (Status == DriverStatus.OnTrip)
        {
            return;
        }

        DriverStatus previous = Status;
        Status = DriverStatus.OnTrip;
        DateTime occurredOn = DateTime.UtcNow;
        UpdateTime = occurredOn;
        RaiseDomainEvent(new DriverStatusChangedEvent(Id, previous, Status, occurredOn));
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Name is required.", nameof(newName));
        }

        string trimmed = newName.Trim();
        if (trimmed.Equals(Name, StringComparison.Ordinal))
        {
            return;
        }

        string oldName = Name;
        Name = trimmed;
        DateTime occurredOn = DateTime.UtcNow;
        UpdateTime = occurredOn;
        RaiseDomainEvent(new DriverRenamedEvent(Id, oldName, Name, occurredOn));
    }
}
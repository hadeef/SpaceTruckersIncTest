using SpaceTruckersInc.Domain.Common;
using SpaceTruckersInc.Domain.Enums;
using SpaceTruckersInc.Domain.Events;
using SpaceTruckersInc.Domain.Exceptions;

namespace SpaceTruckersInc.Domain.Entities;

public class Vehicle : Entity
{
    public Vehicle(VehicleModel model, decimal cargoCapacity)
    {
        Model = model ?? throw new ArgumentException("Model is required.", nameof(model));
        if (cargoCapacity < 0)
        {
            throw new ArgumentException("CargoCapacity must be non-negative.", nameof(cargoCapacity));
        }
        CargoCapacity = cargoCapacity;
        Condition = VehicleCondition.Functional;
        Status = VehicleStatus.Available;
    }

    public decimal CargoCapacity { get; private set; }
    public VehicleCondition Condition { get; private set; }
    public VehicleModel Model { get; private set; }
    public VehicleStatus Status { get; private set; }

    public void AssignToTrip()
    {
        if (Status == VehicleStatus.OnTrip)
        {
            throw new VehicleAlreadyOnTripException(Id);
        }

        if (Condition == VehicleCondition.Damaged)
        {
            throw new VehicleDamagedException(Id);
        }

        VehicleStatus previousStatus = Status;
        VehicleCondition previousCondition = Condition;

        Status = VehicleStatus.OnTrip;
        DateTime occurredOn = DateTime.UtcNow;
        UpdateTime = occurredOn;
        RaiseDomainEvent(new VehicleStateChangedEvent(Id, previousStatus, Status, previousCondition, Condition, occurredOn));
    }

    public void MarkDamaged()
    {
        VehicleStatus previousStatus = Status;
        VehicleCondition previousCondition = Condition;

        Condition = VehicleCondition.Damaged;
        Status = VehicleStatus.Maintenance;
        DateTime occurredOn = DateTime.UtcNow;
        UpdateTime = occurredOn;
        RaiseDomainEvent(new VehicleStateChangedEvent(Id, previousStatus, Status, previousCondition, Condition, occurredOn));
    }

    public void ReleaseFromTrip()
    {
        VehicleStatus previousStatus = Status;
        VehicleCondition previousCondition = Condition;

        Status = VehicleStatus.Available;
        DateTime occurredOn = DateTime.UtcNow;
        UpdateTime = occurredOn;
        RaiseDomainEvent(new VehicleStateChangedEvent(Id, previousStatus, Status, previousCondition, Condition, occurredOn));
    }

    public void Repair()
    {
        VehicleStatus previousStatus = Status;
        VehicleCondition previousCondition = Condition;

        Condition = VehicleCondition.Functional;
        Status = VehicleStatus.Available;
        DateTime occurredOn = DateTime.UtcNow;
        UpdateTime = occurredOn;
        RaiseDomainEvent(new VehicleStateChangedEvent(Id, previousStatus, Status, previousCondition, Condition, occurredOn));
    }
}
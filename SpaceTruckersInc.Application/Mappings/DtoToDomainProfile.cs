using AutoMapper;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Application.DTOs.Requests;
using SpaceTruckersInc.Domain.Entities;
using SpaceTruckersInc.Domain.Enums;

namespace SpaceTruckersInc.Application.Mappings;

public sealed class DtoToDomainProfile : Profile
{
    public DtoToDomainProfile()
    {
        #region Requests

        _ = CreateMap<CreateVehicleRequest, Vehicle>()
            .ConstructUsing(src => new Vehicle(VehicleModel.FromName(src.Model, false), src.CargoCapacity));

        _ = CreateMap<CreateDriverRequest, Driver>()
            .ConstructUsing(src => new Driver(src.Name, LicenseLevel.FromName(src.LicenseLevel, false)));

        _ = CreateMap<CreateRouteRequest, Route>()
            .ConstructUsing(src => new Route(src.Origin, src.Destination, src.EstimatedDuration, src.Checkpoints));

        #endregion Requests

        #region Entities

        _ = CreateMap<DriverDto, Driver>()
            .ConstructUsing(src => new Driver(src.Name, LicenseLevel.FromName(src.LicenseLevel, false)))
            .ForMember(d => d.LicenseLevel, o => o.Ignore())
            .ForMember(d => d.Status, o => o.MapFrom(src =>
                string.IsNullOrWhiteSpace(src.Status) ? DriverStatus.Available : DriverStatus.FromName(src.Status, false)))
            .ForMember(d => d.RowVersion, o => o.MapFrom(s => s.RowVersion))
            .AfterMap((src, dest) =>
            {
                if (!string.IsNullOrWhiteSpace(src.Name) && src.Name != dest.Name)
                {
                    dest.Rename(src.Name);
                }

                if (!string.IsNullOrWhiteSpace(src.LicenseLevel))
                {
                    try
                    {
                        LicenseLevel newLevel = LicenseLevel.FromName(src.LicenseLevel, false);
                        dest.ChangeLicense(newLevel);
                    }
                    catch
                    {
                    }
                }

                if (!string.IsNullOrWhiteSpace(src.Status))
                {
                    try
                    {
                        DriverStatus status = DriverStatus.FromName(src.Status, false);
                        if (status == DriverStatus.OnTrip)
                        {
                            dest.MarkOnTrip();
                        }
                        else if (status == DriverStatus.Available)
                        {
                            dest.MarkAvailable();
                        }
                    }
                    catch
                    {
                    }
                }
            });

        _ = CreateMap<VehicleDto, Vehicle>()
            .ConstructUsing(src => new Vehicle(VehicleModel.FromName(src.Model, false), src.CargoCapacity))
            .ForMember(d => d.Model, o => o.Ignore())
            .ForMember(d => d.CargoCapacity, o => o.Ignore())
            .ForMember(d => d.Condition, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.RowVersion, o => o.MapFrom(s => s.RowVersion))
            .AfterMap((src, dest) =>
            {
                // Apply condition first
                try
                {
                    if (!string.IsNullOrWhiteSpace(src.Condition))
                    {
                        VehicleCondition condition = VehicleCondition.FromName(src.Condition, false);
                        if (condition == VehicleCondition.Damaged && dest.Condition != VehicleCondition.Damaged)
                        {
                            dest.MarkDamaged(); // sets Maintenance status
                        }
                        else if (condition == VehicleCondition.Functional && dest.Condition != VehicleCondition.Functional)
                        {
                            dest.Repair(); // sets Available status
                        }
                    }
                }
                catch
                {
                }

                // Apply status with guard: damaged must not be available
                try
                {
                    if (!string.IsNullOrWhiteSpace(src.Status))
                    {
                        VehicleStatus status = VehicleStatus.FromName(src.Status, false);

                        if (status == VehicleStatus.OnTrip && dest.Status != VehicleStatus.OnTrip)
                        {
                            dest.AssignToTrip();
                        }
                        else if (status == VehicleStatus.Available && dest.Condition != VehicleCondition.Damaged && dest.Status != VehicleStatus.Available)
                        {
                            dest.ReleaseFromTrip();
                        }
                        else if (status == VehicleStatus.Maintenance && dest.Status != VehicleStatus.Maintenance)
                        {
                            // Keep condition + status aligned to maintenance
                            if (dest.Condition != VehicleCondition.Damaged)
                            {
                                dest.MarkDamaged(); // sets Maintenance
                            }
                            else
                            {
                                dest.MarkDamaged(); // already damaged; keeps Maintenance
                            }
                        }
                        else if (status == VehicleStatus.Available && dest.Condition == VehicleCondition.Damaged)
                        {
                            // Requested Available but condition is Damaged: enforce maintenance
                            dest.MarkDamaged();
                        }
                    }
                }
                catch
                {
                }
            });

        _ = CreateMap<RouteDto, Route>()
            .ConstructUsing(src => new Route(src.Origin, src.Destination, src.EstimatedDuration, src.Checkpoints))
            .ForMember(d => d.Origin, o => o.Ignore())
            .ForMember(d => d.Destination, o => o.Ignore())
            .ForMember(d => d.EstimatedDuration, o => o.Ignore())
            .ForMember(d => d.Checkpoints, o => o.Ignore())
            .ForMember(d => d.RowVersion, o => o.MapFrom(s => s.RowVersion))
            .AfterMap((src, dest) =>
            {
                try
                {
                    if (src.EstimatedDuration.TotalSeconds >= 0 && src.EstimatedDuration != dest.EstimatedDuration)
                    {
                        dest.UpdateEstimatedDuration(src.EstimatedDuration);
                    }
                }
                catch
                {
                }

                try
                {
                    List<string> existingCheckpoints = dest.Checkpoints.ToList();
                    IReadOnlyList<string> incomingCheckpoints = src.Checkpoints ?? Array.Empty<string>();

                    foreach (string cp in incomingCheckpoints)
                    {
                        if (!existingCheckpoints.Contains(cp, StringComparer.Ordinal))
                        {
                            dest.AddCheckpoint(cp);
                        }
                    }

                    foreach (string cp in existingCheckpoints)
                    {
                        if (!incomingCheckpoints.Contains(cp, StringComparer.Ordinal))
                        {
                            _ = dest.RemoveCheckpoint(cp);
                        }
                    }
                }
                catch
                {
                }
            });

        _ = CreateMap<TripEventDto, TripEvent>()
            .ConstructUsing(src => new TripEvent(
                src.Id == Guid.Empty ? Guid.NewGuid() : src.Id,
                src.OccurredOn == default ? DateTime.UtcNow : src.OccurredOn,
                string.IsNullOrWhiteSpace(src.EventType) ? TripEventType.Other : TripEventType.FromName(src.EventType, false),
                src.Details));

        #endregion Entities
    }
}
using Microsoft.Extensions.Logging;
using SpaceTruckersInc.Domain.Common.Interfaces;
using SpaceTruckersInc.Domain.Entities;
using SpaceTruckersInc.Domain.Interfaces;

namespace SpaceTruckersInc.Infrastructure.Repositories;

public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
{
    public VehicleRepository(ApplicationDbContext context, IDomainEventDispatcher domainEventDispatcher, ILogger<VehicleRepository> logger)
        : base(context, domainEventDispatcher, logger)
    {
    }
}
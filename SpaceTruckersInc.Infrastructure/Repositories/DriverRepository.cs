using Microsoft.Extensions.Logging;
using SpaceTruckersInc.Domain.Common.Interfaces;
using SpaceTruckersInc.Domain.Entities;
using SpaceTruckersInc.Domain.Interfaces;

namespace SpaceTruckersInc.Infrastructure.Repositories;

public class DriverRepository : Repository<Driver>, IDriverRepository
{
    public DriverRepository(ApplicationDbContext context, IDomainEventDispatcher domainEventDispatcher, ILogger<DriverRepository> logger)
        : base(context, domainEventDispatcher, logger)
    {
    }
}
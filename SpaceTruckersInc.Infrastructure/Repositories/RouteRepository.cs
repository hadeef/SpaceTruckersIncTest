using Microsoft.Extensions.Logging;
using SpaceTruckersInc.Domain.Common.Interfaces;
using SpaceTruckersInc.Domain.Entities;
using SpaceTruckersInc.Domain.Interfaces;

namespace SpaceTruckersInc.Infrastructure.Repositories;

public class RouteRepository : Repository<Route>, IRouteRepository
{
    public RouteRepository(ApplicationDbContext context, IDomainEventDispatcher domainEventDispatcher, ILogger<RouteRepository> logger)
        : base(context, domainEventDispatcher, logger)
    {
    }
}
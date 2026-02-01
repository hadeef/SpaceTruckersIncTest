using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpaceTruckersInc.Domain.Common.Interfaces;
using SpaceTruckersInc.Domain.Entities;
using SpaceTruckersInc.Domain.Interfaces;

namespace SpaceTruckersInc.Infrastructure.Repositories;

public class TripRepository : Repository<Trip>, ITripRepository
{
    public TripRepository(ApplicationDbContext context, IDomainEventDispatcher domainEventDispatcher, ILogger<TripRepository> logger)
        : base(context, domainEventDispatcher, logger)
    {
    }

    public async Task<Trip?> GetByIdWithTimelineAsync(Guid id)
    {
        return await _context.Trips
            .Include(t => t.TripEvents)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}
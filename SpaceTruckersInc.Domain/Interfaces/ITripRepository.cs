using SpaceTruckersInc.Domain.Entities;

namespace SpaceTruckersInc.Domain.Interfaces;

public interface ITripRepository : IRepository<Trip>
{
    /// <summary>
    /// Returns the trip including timeline/details useful for read-models or summaries.
    /// </summary>
    Task<Trip?> GetByIdWithTimelineAsync(Guid id);
}
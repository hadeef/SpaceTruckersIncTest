using SpaceTruckersInc.Domain.Common;

namespace SpaceTruckersInc.Domain.Interfaces;

/// <summary>
/// Generic repository contract for domain entities.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public interface IRepository<T> where T : Entity
{
    Task<T> AddAsync(T entity);

    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

    Task DeleteAsync(T entity);

    Task DeleteRangeAsync(IEnumerable<T> entities);

    Task<IEnumerable<T>> GetAllAsync();

    IQueryable<T> GetAllQueryable();

    Task<T?> GetByIdAsync(Guid id);

    Task SaveChangesAsync();

    Task<T> UpdateAsync(T entity);

    Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities);
}
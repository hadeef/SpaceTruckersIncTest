using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using SpaceTruckersInc.Domain.Common;
using SpaceTruckersInc.Domain.Common.Interfaces;
using SpaceTruckersInc.Domain.Exceptions;
using SpaceTruckersInc.Domain.Interfaces;

namespace SpaceTruckersInc.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly ILogger<Repository<TEntity>> _logger;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public Repository(
        ApplicationDbContext context,
        IDomainEventDispatcher domainEventDispatcher,
        ILogger<Repository<TEntity>> logger)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
        _domainEventDispatcher = domainEventDispatcher;
        _logger = logger;
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        _logger.LogInformation(
            "Adding a new {EntityType} entity.",
            typeof(TEntity).Name);

        _ = _dbSet.Add(entity);
        await SaveChangesWithConcurrencyHandlingAsync();

        TEntity? saved = await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.Id);
        return saved ?? entity;
    }

    public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
    {
        List<TEntity> entityList = entities.ToList();
        _logger.LogInformation(
            "Adding {Count} {EntityType} entities.",
            entityList.Count,
            typeof(TEntity).Name);

        _dbSet.AddRange(entityList);
        await SaveChangesWithConcurrencyHandlingAsync();

        List<Guid> ids = entityList.Select(e => e.Id).ToList();
        List<TEntity> persistedEntities = await _dbSet.AsNoTracking().Where(e => ids.Contains(e.Id)).ToListAsync();
        return persistedEntities;
    }

    public virtual async Task DeleteAsync(TEntity entity)
    {
        _logger.LogInformation(
            "Deleting {EntityType} entity.",
            typeof(TEntity).Name);

        if (_context.Entry(entity).State == EntityState.Detached)
        {
            _ = _dbSet.Attach(entity);
        }
        _context.Entry(entity).Property(e => e.RowVersion).OriginalValue = entity.RowVersion;
        _ = _dbSet.Remove(entity);
        await SaveChangesWithConcurrencyHandlingAsync();
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities)
    {
        List<TEntity> entityList = entities.ToList();
        _logger.LogInformation(
            "Deleting {Count} {EntityType} entities.",
            entityList.Count,
            typeof(TEntity).Name);

        foreach (TEntity entity in entityList)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _ = _dbSet.Attach(entity);
            }
            _context.Entry(entity).Property(e => e.RowVersion).OriginalValue = entity.RowVersion;
        }

        _dbSet.RemoveRange(entityList);
        await SaveChangesWithConcurrencyHandlingAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        _logger.LogInformation(
            "Fetching all {EntityType} entities.",
            typeof(TEntity).Name);

        List<TEntity> result = await _dbSet.AsNoTracking().ToListAsync();
        return result;
    }

    public IQueryable<TEntity> GetAllQueryable()
    {
        _logger.LogInformation(
            "Fetching all {EntityType} entities.",
            typeof(TEntity).Name);

        IQueryable<TEntity> result = _dbSet.AsNoTracking();
        return result;
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation(
            "Fetching {EntityType} entity with ID {Id}.",
            typeof(TEntity).Name,
            id);

        TEntity? result = await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        return result;
    }

    public async Task SaveChangesAsync()
    {
        await SaveChangesWithConcurrencyHandlingAsync();
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        _logger.LogInformation("Updating {EntityType} entity.", typeof(TEntity).Name);

        EntityEntry<TEntity> entry = _context.Entry(entity);

        if (entry.State == EntityState.Detached)
        {
            _ = _dbSet.Attach(entity);
            entry = _context.Entry(entity);
            entry.State = EntityState.Modified;
        }

        // Always set the original rowversion so concurrency check uses the incoming token (or
        // refreshed one).
        entry.Property(e => e.RowVersion).OriginalValue = entity.RowVersion;

        await SaveChangesWithConcurrencyHandlingAsync();

        TEntity? saved = await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.Id);
        return saved ?? entity;
    }

    public virtual async Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        List<TEntity> entityList = entities.ToList();
        _logger.LogInformation("Updating {Count} {EntityType} entities.", entityList.Count, typeof(TEntity).Name);

        foreach (TEntity entity in entityList)
        {
            EntityEntry<TEntity> entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _ = _dbSet.Attach(entity);
                entry = _context.Entry(entity);
                entry.State = EntityState.Modified;
            }

            entry.Property(e => e.RowVersion).OriginalValue = entity.RowVersion;
        }

        await SaveChangesWithConcurrencyHandlingAsync();

        List<Guid> ids = entityList.Select(e => e.Id).ToList();
        List<TEntity> persistedEntities = await _dbSet.AsNoTracking().Where(e => ids.Contains(e.Id)).ToListAsync();
        return persistedEntities;
    }

    #region Private Methods

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        List<Entity> entities = _context.ChangeTracker
            .Entries<Entity>()
            .Select(e => e.Entity)
            .ToList();

        List<IDomainEvent> domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        if (domainEvents.Count == 0)
        {
            return;
        }

        entities.ForEach(e => e.ClearDomainEvents());

        await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
    }

    private void EnsureUpdateTimes()
    {
        DateTime now = DateTime.UtcNow;

        List<EntityEntry<Entity>> pendingEntities = _context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.State == EntityState.Modified)
            .ToList();

        foreach (EntityEntry<Entity> entry in pendingEntities)
        {
            entry.Entity.UpdateTime = now;
        }
    }

    private async Task SaveChangesWithConcurrencyHandlingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            EnsureUpdateTimes();
            _ = await _context.SaveChangesAsync(cancellationToken);
            await DispatchDomainEventsAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Guid[] conflictingIds = ex.Entries
                .Select(e => e.Entity)
                .OfType<Entity>()
                .Select(e => e.Id)
                .Distinct()
                .ToArray();

            _logger.LogWarning(ex, "Concurrency conflict detected for {EntityType} ids: {Ids}."
                , typeof(TEntity).Name, string.Join(',', conflictingIds));
            throw new ConcurrencyConflictException(
                $"Concurrency conflict for {typeof(TEntity).Name}. Conflicting ids: {string.Join(',', conflictingIds)}", ex);
        }
    }

    #endregion Private Methods
}
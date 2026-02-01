using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpaceTruckersInc.Application.Common;
using SpaceTruckersInc.Application.Common.Interfaces;
using SpaceTruckersInc.Domain.Common;
using SpaceTruckersInc.Domain.Enums;
using SpaceTruckersInc.Domain.Exceptions;
using SpaceTruckersInc.Domain.Interfaces;
using System.Reflection;

namespace SpaceTruckersInc.Application.Services;

public abstract class EntityService<TEntity, TDto, TRepository>
    where TEntity : Entity
    where TRepository : IRepository<TEntity>
{
    protected readonly ICachingService _cache;
    protected readonly ILogger _logger;
    protected readonly IMapper _mapper;
    protected readonly TRepository _repository;

    private const int MaxConcurrencyRetryAttempts = 3;

    protected EntityService(TRepository repository, IMapper mapper, ICachingService cache, ILogger logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Public DTO-facing API

    public virtual async Task<ServiceResponse<TDto>> AddAndSaveAsync(TDto dto, string logMessageTemplate, params object[] logArgs)
    {
        ServiceResponse<TDto> response = new();
        try
        {
            if (dto is null)
            {
                response.Errors.Add("Request body is required.");
                response.StatusCode = ServiceResponseStatus.BadRequest.Value;
                return response;
            }

            TEntity entity = _mapper.Map<TEntity>(dto);
            TEntity saved = await AddEntityAndSaveAsync(entity, logMessageTemplate, logArgs);
            response.Data = _mapper.Map<TDto>(saved);
            response.StatusCode = ServiceResponseStatus.Created.Value;
            return response;
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict in AddAndSaveAsync for {EntityType}", typeof(TEntity).Name);
            ServiceResponse<TDto> conflictResponse = new();
            conflictResponse.Errors.Add("Concurrency conflict occurred while adding the entity.");
            conflictResponse.StatusCode = ServiceResponseStatus.Conflict.Value;
            return conflictResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddAndSaveAsync failed for {EntityType}", typeof(TEntity).Name);
            const string friendlyErroMessage = "An unexpected error occurred while adding the entity.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            return response;
        }
    }

    public virtual async Task<ServiceResponse<IEnumerable<TDto>>> AddRangeAndSaveAsync(IEnumerable<TDto> dtos, string logMessageTemplate
        , params object[] logArgs)
    {
        ServiceResponse<IEnumerable<TDto>> response = new();
        try
        {
            if (dtos is null)
            {
                response.Errors.Add("Request body is required.");
                response.StatusCode = ServiceResponseStatus.BadRequest.Value;
                return response;
            }

            List<TEntity> entities = _mapper.Map<List<TEntity>>(dtos);
            IEnumerable<TEntity> saved = await AddEntityRangeAndSaveAsync(entities, logMessageTemplate, logArgs);
            response.Data = _mapper.Map<IEnumerable<TDto>>(saved);
            response.StatusCode = ServiceResponseStatus.Created.Value;
            return response;
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict in AddRangeAndSaveAsync for {EntityType}", typeof(TEntity).Name);
            ServiceResponse<IEnumerable<TDto>> conflictResponse = new();
            conflictResponse.Errors.Add("Concurrency conflict occurred while adding entities.");
            conflictResponse.StatusCode = ServiceResponseStatus.Conflict.Value;
            return conflictResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddRangeAndSaveAsync failed for {EntityType}", typeof(TEntity).Name);
            const string friendlyErroMessage = "An unexpected error occurred while adding entities.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            return response;
        }
    }

    public virtual async Task<ServiceResponse<bool>> DeleteAndSaveAsync(Guid id, string logMessageTemplate, params object[] logArgs)
    {
        ServiceResponse<bool> response = new();
        try
        {
            TEntity? entity = await _repository.GetByIdAsync(id);
            if (entity is null)
            {
                response.Data = false;
                response.StatusCode = ServiceResponseStatus.NotFound.Value;
                return response;
            }

            await DeleteEntityAndSaveAsync(entity, logMessageTemplate, logArgs);
            response.Data = true;
            response.StatusCode = ServiceResponseStatus.NoContent.Value;
            return response;
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict in DeleteAndSaveAsync for {EntityType} id {Id}", typeof(TEntity).Name, id);
            ServiceResponse<bool> conflictResponse = new();
            conflictResponse.Errors.Add("Concurrency conflict occurred while deleting the entity.");
            conflictResponse.StatusCode = ServiceResponseStatus.Conflict.Value;
            conflictResponse.Data = false;
            return conflictResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteAndSaveAsync failed for {EntityType} id {Id}", typeof(TEntity).Name, id);
            const string friendlyErroMessage = "An unexpected error occurred while deleting the entity.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            response.Data = false;
            return response;
        }
    }

    public virtual async Task<ServiceResponse<bool>> DeleteRangeAndSaveAsync(IEnumerable<Guid> ids, string logMessageTemplate, params object[] logArgs)
    {
        ServiceResponse<bool> response = new();
        try
        {
            if (ids is null)
            {
                response.Errors.Add("Request body is required.");
                response.StatusCode = ServiceResponseStatus.BadRequest.Value;
                response.Data = false;
                return response;
            }

            List<Guid> idList = ids.ToList();
            if (idList.Count == 0)
            {
                response.Data = true;
                response.StatusCode = ServiceResponseStatus.NoContent.Value;
                return response;
            }

            IEnumerable<TEntity> entities = await _repository.GetAllAsync();
            List<TEntity> toDelete = entities.Where(e => idList.Contains(e.Id)).ToList();
            if (toDelete.Count == 0)
            {
                response.Data = false;
                response.StatusCode = ServiceResponseStatus.NotFound.Value;
                return response;
            }

            await DeleteEntityRangeAndSaveAsync(toDelete, logMessageTemplate, logArgs);
            response.Data = true;
            response.StatusCode = ServiceResponseStatus.NoContent.Value;
            return response;
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict in DeleteRangeAndSaveAsync for {EntityType}", typeof(TEntity).Name);
            ServiceResponse<bool> conflictResponse = new();
            conflictResponse.Errors.Add("Concurrency conflict occurred while deleting entities.");
            conflictResponse.StatusCode = ServiceResponseStatus.Conflict.Value;
            conflictResponse.Data = false;
            return conflictResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteRangeAndSaveAsync failed for {EntityType}", typeof(TEntity).Name);
            const string friendlyErroMessage = "An unexpected error occurred while deleting entities.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            response.Data = false;
            return response;
        }
    }

    public virtual async Task<ServiceResponse<IEnumerable<TDto>?>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        ServiceResponse<IEnumerable<TDto>?> response = new();
        try
        {
            IEnumerable<TEntity> entities = await _repository.GetAllAsync();
            response.Data = _mapper.Map<IEnumerable<TDto>>(entities);
            response.StatusCode = ServiceResponseStatus.Success.Value;
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllAsync failed for {EntityType}", typeof(TEntity).Name);
            const string friendlyErroMessage = "An unexpected error occurred while fetching entities.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            response.Data = null;
            return response;
        }
    }

    public virtual async Task<ServiceResponse<IEnumerable<TDto>?>> GetAllCachedAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        ServiceResponse<IEnumerable<TDto>?> response = new();
        try
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                response.Errors.Add("cacheKey is required.");
                response.StatusCode = ServiceResponseStatus.BadRequest.Value;
                response.Data = null;
                return response;
            }

            IEnumerable<TDto>? data = await _cache.GetOrAddCacheAsync(async () =>
            {
                IEnumerable<TEntity> entities = await _repository.GetAllAsync();
                return _mapper.Map<IEnumerable<TDto>>(entities);
            }, uniqueIdentity: cacheKey);

            response.Data = data;
            response.StatusCode = ServiceResponseStatus.Success.Value;
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllCachedAsync failed for {EntityType} cacheKey {CacheKey}", typeof(TEntity).Name, cacheKey);
            const string friendlyErroMessage = "An unexpected error occurred while fetching cached entities.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            response.Data = null;
            return response;
        }
    }

    public virtual async Task<ServiceResponse<TDto?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ServiceResponse<TDto?> response = new();
        try
        {
            TEntity? entity = await _repository.GetByIdAsync(id);
            response.Data = entity is null ? default : _mapper.Map<TDto>(entity);
            response.StatusCode = entity is null ? ServiceResponseStatus.NotFound.Value : ServiceResponseStatus.Success.Value;
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetByIdAsync failed for {EntityType} id {Id}", typeof(TEntity).Name, id);
            const string friendlyErroMessage = "An unexpected error occurred while fetching the entity.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            response.Data = default;
            return response;
        }
    }

    public virtual async Task<ServiceResponse<TDto>> UpdateAndSaveAsync(TDto dto, string logMessageTemplate, params object[] logArgs)
    {
        ServiceResponse<TDto> response = new();
        try
        {
            if (dto is null)
            {
                response.Errors.Add("Request body is required.");
                response.StatusCode = ServiceResponseStatus.BadRequest.Value;
                return response;
            }

            Guid? dtoId = null;
            PropertyInfo? idProperty = typeof(TDto).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            if (idProperty is not null && idProperty.PropertyType == typeof(Guid))
            {
                object? idValue = idProperty.GetValue(dto);
                if (idValue is Guid g && g != Guid.Empty)
                {
                    dtoId = g;
                }
            }

            TEntity entityToPersist;
            if (dtoId.HasValue)
            {
                TEntity? existingEntity = await _repository.GetByIdAsync(dtoId.Value);
                if (existingEntity is null)
                {
                    response.Errors.Add("Entity not found.");
                    response.StatusCode = ServiceResponseStatus.NotFound.Value;
                    return response;
                }

                _ = _mapper.Map(dto, existingEntity);
                entityToPersist = existingEntity;
            }
            else
            {
                entityToPersist = _mapper.Map<TEntity>(dto);
            }

            TEntity saved = await ExecuteWithConcurrencyRetryAsync(
                async () => await PerformUpdateAsync(entityToPersist),
                entityToRefresh: entityToPersist,
                maxAttempts: MaxConcurrencyRetryAttempts);

            _logger.LogInformation(logMessageTemplate, logArgs);
            response.Data = _mapper.Map<TDto>(saved);
            response.StatusCode = ServiceResponseStatus.Success.Value;
            return response;
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict in UpdateAndSaveAsync for {EntityType}", typeof(TEntity).Name);
            ServiceResponse<TDto> conflictResponse = new();
            conflictResponse.Errors.Add("Concurrency conflict occurred while updating the entity.");
            conflictResponse.StatusCode = ServiceResponseStatus.Conflict.Value;
            return conflictResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateAndSaveAsync failed for {EntityType}", typeof(TEntity).Name);
            const string friendlyErroMessage = "An unexpected error occurred while updating the entity.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            return response;
        }
    }

    public virtual async Task<ServiceResponse<IEnumerable<TDto>>> UpdateRangeAndSaveAsync(IEnumerable<TDto> dtos
        , string logMessageTemplate, params object[] logArgs)
    {
        ServiceResponse<IEnumerable<TDto>> response = new();
        try
        {
            if (dtos is null)
            {
                response.Errors.Add("Request body is required.");
                response.StatusCode = ServiceResponseStatus.BadRequest.Value;
                return response;
            }

            List<TEntity> entities = _mapper.Map<List<TEntity>>(dtos);

            IEnumerable<TEntity> saved = await ExecuteWithConcurrencyRetryAsync(
                async () => await PerformUpdateRangeAsync(entities),
                entityToRefresh: null,
                maxAttempts: MaxConcurrencyRetryAttempts);

            _logger.LogInformation(logMessageTemplate, logArgs);
            response.Data = _mapper.Map<IEnumerable<TDto>>(saved);
            response.StatusCode = ServiceResponseStatus.Success.Value;
            return response;
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict in UpdateRangeAndSaveAsync for {EntityType}", typeof(TEntity).Name);
            ServiceResponse<IEnumerable<TDto>> conflictResponse = new();
            conflictResponse.Errors.Add("Concurrency conflict occurred while updating entities.");
            conflictResponse.StatusCode = ServiceResponseStatus.Conflict.Value;
            return conflictResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateRangeAndSaveAsync failed for {EntityType}", typeof(TEntity).Name);
            const string friendlyErroMessage = "An unexpected error occurred while updating entities.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            return response;
        }
    }

    #endregion Public DTO-facing API

    #region Protected entity-based helpers

    protected virtual async Task<TEntity> AddEntityAndSaveAsync(TEntity entity, string logMessageTemplate, params object[] logArgs)
    {
        try
        {
            TEntity saved = await _repository.AddAsync(entity);
            _logger.LogInformation(logMessageTemplate, logArgs);
            return saved;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddEntityAndSaveAsync failed for {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    protected virtual async Task<IEnumerable<TEntity>> AddEntityRangeAndSaveAsync(IEnumerable<TEntity> entities
        , string logMessageTemplate, params object[] logArgs)
    {
        try
        {
            IEnumerable<TEntity> saved = await _repository.AddRangeAsync(entities);
            _logger.LogInformation(logMessageTemplate, logArgs);
            return saved;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddEntityRangeAndSaveAsync failed for {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    protected virtual async Task DeleteEntityAndSaveAsync(TEntity entity, string logMessageTemplate, params object[] logArgs)
    {
        try
        {
            await PerformDeleteAsync(entity);
            _logger.LogInformation(logMessageTemplate, logArgs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteEntityAndSaveAsync failed for {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    protected virtual async Task DeleteEntityRangeAndSaveAsync(IEnumerable<TEntity> entities, string logMessageTemplate, params object[] logArgs)
    {
        try
        {
            await PerformDeleteRangeAsync(entities);
            _logger.LogInformation(logMessageTemplate, logArgs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteEntityRangeAndSaveAsync failed for {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    protected virtual Task<TEntity?> FindEntityByIdAsync(Guid id)
    {
        try
        {
            return _repository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FindEntityByIdAsync failed for {EntityType} id {Id}", typeof(TEntity).Name, id);
            throw;
        }
    }

    protected virtual async Task<TEntity> UpdateEntityAndSaveAsync(TEntity entity, string logMessageTemplate, params object[] logArgs)
    {
        try
        {
            TEntity saved = await ExecuteWithConcurrencyRetryAsync(
                async () => await PerformUpdateAsync(entity),
                entityToRefresh: entity,
                maxAttempts: MaxConcurrencyRetryAttempts);

            _logger.LogInformation(logMessageTemplate, logArgs);
            return saved;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateEntityAndSaveAsync failed for {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    protected virtual async Task<IEnumerable<TEntity>> UpdateEntityRangeAndSaveAsync(IEnumerable<TEntity> entities
        , string logMessageTemplate, params object[] logArgs)
    {
        try
        {
            IEnumerable<TEntity> saved = await PerformUpdateRangeAsync(entities);
            _logger.LogInformation(logMessageTemplate, logArgs);
            return saved;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateEntityRangeAndSaveAsync failed for {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    #endregion Protected entity-based helpers

    #region Internal helpers

    private static bool _contextAccessAvailable()
    {
        return true;
    }

    private async Task<TResult> ExecuteWithConcurrencyRetryAsync<TResult>(
            Func<Task<TResult>> operation,
        TEntity? entityToRefresh = null,
        int maxAttempts = MaxConcurrencyRetryAttempts,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        maxAttempts = Math.Max(1, maxAttempts);

        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (ConcurrencyConflictException ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(
                    ex,
                    "Concurrency conflict on attempt {Attempt}/{MaxAttempts} for {EntityType} id {EntityId}.",
                    attempt,
                    maxAttempts,
                    typeof(TEntity).Name,
                    entityToRefresh?.Id.ToString() ?? "N/A");

                if (entityToRefresh is not null)
                {
                    try
                    {
                        await RefreshTokenAsync(entityToRefresh);
                    }
                    catch (EntityNotFoundException)
                    {
                        throw;
                    }
                }

                int delayMs = Math.Min(200, 50 * attempt);
                await Task.Delay(delayMs, cancellationToken);
            }
            catch (ConcurrencyConflictException ex)
            {
                if (entityToRefresh is not null)
                {
                    throw new ConcurrencyConflictException(entityToRefresh.Id, ex.Message);
                }

                if (ex.EntityId != Guid.Empty)
                {
                    throw new ConcurrencyConflictException(ex.EntityId, ex.Message);
                }

                throw new ConcurrencyConflictException(ex.Message, ex);
            }
        }
    }

    private Task PerformDeleteAsync(TEntity entity)
    {
        try
        {
            return _repository.DeleteAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PerformDeleteAsync failed for {EntityType} id {Id}", typeof(TEntity).Name, entity.Id);
            throw;
        }
    }

    private Task PerformDeleteRangeAsync(IEnumerable<TEntity> entities)
    {
        try
        {
            return _repository.DeleteRangeAsync(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PerformDeleteRangeAsync failed for {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    private Task<TEntity> PerformUpdateAsync(TEntity entity)
    {
        try
        {
            return _repository.UpdateAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PerformUpdateAsync failed for {EntityType} id {Id}", typeof(TEntity).Name, entity.Id);
            throw;
        }
    }

    private Task<IEnumerable<TEntity>> PerformUpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        try
        {
            return _repository.UpdateRangeAsync(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PerformUpdateRangeAsync failed for {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    private async Task RefreshTokenAsync(TEntity entity)
    {
        try
        {
            TEntity? latest = await _repository.GetAllQueryable().FirstOrDefaultAsync(e => e.Id == entity.Id);
            if (latest is null)
            {
                throw new EntityNotFoundException(typeof(TEntity), entity.Id);
            }

            entity.RowVersion = latest.RowVersion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RefreshTokenAsync failed for {EntityType} id {Id}", typeof(TEntity).Name, entity.Id);
            throw;
        }
    }

    #endregion Internal helpers
}
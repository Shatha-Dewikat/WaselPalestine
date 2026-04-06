using Mapster;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Repository;

namespace Wasel_Palestine.BAL.Service
{
    public class IncidentStatusService : IIncidentStatusService
    {
        private readonly IIncidentStatusRepository _repository;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string StatusListCacheKey = "IncidentStatuses_List";

        public IncidentStatusService(IIncidentStatusRepository repository, ApplicationDbContext context, IMemoryCache cache)
        {
            _repository = repository;
            _context = context;
            _cache = cache;
        }

        private void ClearStatusCache()
        {
            _cache.Remove(StatusListCacheKey);
        }

        public async Task<List<IncidentStatusResponse>> GetAllStatusesAsync()
        {
            if (!_cache.TryGetValue(StatusListCacheKey, out List<IncidentStatusResponse> cachedList))
            {
                var statuses = await _repository.GetAllAsync();
                cachedList = statuses.Adapt<List<IncidentStatusResponse>>();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(24))
                    .SetSlidingExpiration(TimeSpan.FromHours(4));

                _cache.Set(StatusListCacheKey, cachedList, cacheOptions);
            }
            return cachedList;
        }

        public async Task<IncidentStatusResponse> GetStatusByIdAsync(int id)
        {
          
            var list = await GetAllStatusesAsync();
            return list.Find(s => s.Id == id);
        }

        public async Task<IncidentStatusResponse> CreateStatusAsync(
            IncidentStatusCreateRequest request,
            string userId,
            string ip,
            string userAgent)
        {
            if (await _repository.ExistsByNameAsync(request.Name))
                throw new InvalidOperationException($"Status '{request.Name}' already exists.");

            var status = new IncidentStatus
            {
                Name = request.Name,
                Description = request.Description
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _repository.AddAsync(status);

                await _context.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "Create",
                    EntityName = nameof(IncidentStatus),
                    EntityId = result.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Created status: {result.Name}",
                    IPAddress = ip,
                    UserAgent = userAgent
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                ClearStatusCache(); 

                return result.Adapt<IncidentStatusResponse>();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IncidentStatusResponse> UpdateStatusAsync(
            int id,
            IncidentStatusUpdateRequest request,
            string userId,
            string ip,
            string userAgent)
        {
            var status = await _repository.GetByIdAsync(id);
            if (status == null) throw new KeyNotFoundException("Status not found");

            if (await _repository.ExistsByNameAsync(request.Name, excludeId: id))
                throw new InvalidOperationException($"Status '{request.Name}' already exists.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                status.Name = request.Name;
                status.Description = request.Description;
                await _repository.UpdateAsync(status);

                await _context.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "Update",
                    EntityName = nameof(IncidentStatus),
                    EntityId = status.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Created status: {status.Name} with Description: {status.Description}",
                    IPAddress = ip,
                    UserAgent = userAgent
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                ClearStatusCache(); 

                return status.Adapt<IncidentStatusResponse>();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteStatusAsync(int id, string userId, string ip, string userAgent)
        {
            var status = await _repository.GetByIdAsync(id);
            if (status == null) throw new KeyNotFoundException("Status not found");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _repository.DeleteAsync(status);

                await _context.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "Delete",
                    EntityName = nameof(IncidentStatus),
                    EntityId = status.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Deleted status: {status.Name}",
                    IPAddress = ip,
                    UserAgent = userAgent
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                ClearStatusCache(); 
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
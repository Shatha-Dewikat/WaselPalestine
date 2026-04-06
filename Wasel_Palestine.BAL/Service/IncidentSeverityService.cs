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
    public class IncidentSeverityService : IIncidentSeverityService
    {
        private readonly IIncidentSeverityRepository _repository;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        private const string SeverityListCacheKey = "IncidentSeverities_List";

        public IncidentSeverityService(IIncidentSeverityRepository repository, ApplicationDbContext context, IMemoryCache cache)
        {
            _repository = repository;
            _context = context;
            _cache = cache;
        }

        private void ClearSeverityCache()
        {
            _cache.Remove(SeverityListCacheKey);
        }

        public async Task<List<IncidentSeverityResponse>> GetAllIncidentSeveritiesAsync()
        {
            if (!_cache.TryGetValue(SeverityListCacheKey, out List<IncidentSeverityResponse> cachedList))
            {
                var severities = await _repository.GetAllAsync();
                cachedList = severities.Adapt<List<IncidentSeverityResponse>>();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(24))
                    .SetSlidingExpiration(TimeSpan.FromHours(4));

                _cache.Set(SeverityListCacheKey, cachedList, cacheOptions);
            }

            return cachedList;
        }

        public async Task<IncidentSeverityResponse> CreateIncidentSeverityAsync(
            IncidentSeverityCreateRequest request,
            string userId,
            string ip,
            string userAgent)
        {
            if (await _repository.ExistsByNameAsync(request.Name))
                throw new InvalidOperationException($"Severity '{request.Name}' already exists.");

            var severity = new IncidentSeverity
            {
                Name = request.Name,
                Level = request.Level 
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _repository.AddAsync(severity);

                await _context.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "Create",
                    EntityName = nameof(IncidentSeverity),
                    EntityId = result.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Created severity: {result.Name}",
                    IPAddress = ip,
                    UserAgent = userAgent
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                ClearSeverityCache();

                return result.Adapt<IncidentSeverityResponse>();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IncidentSeverityResponse> UpdateIncidentSeverityAsync(
            int id,
            IncidentSeverityUpdateRequest request,
            string userId,
            string ip,
            string userAgent)
        {
            var severity = await _repository.GetByIdAsync(id);
            if (severity == null) throw new KeyNotFoundException("Severity not found");

            if (await _repository.ExistsByNameAsync(request.Name, excludeId: id))
                throw new InvalidOperationException($"Severity '{request.Name}' already exists.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                severity.Name = request.Name;
                severity.Level = request.Level; 
                await _repository.UpdateAsync(severity);

                await _context.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "Update",
                    EntityName = nameof(IncidentSeverity),
                    EntityId = severity.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Updated severity: {severity.Name} with Level: {severity.Level}",
                    IPAddress = ip,
                    UserAgent = userAgent
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                ClearSeverityCache();

                return severity.Adapt<IncidentSeverityResponse>();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteIncidentSeverityAsync(int id, string userId, string ip, string userAgent)
        {
            var severity = await _repository.GetByIdAsync(id);
            if (severity == null) throw new KeyNotFoundException("Severity not found");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _repository.DeleteAsync(severity);

                await _context.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "Delete",
                    EntityName = nameof(IncidentSeverity),
                    EntityId = severity.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Deleted severity: {severity.Name}",
                    IPAddress = ip,
                    UserAgent = userAgent
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                ClearSeverityCache();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IncidentSeverityResponse> GetIncidentSeverityByIdAsync(int id)
        {
            
            var list = await GetAllIncidentSeveritiesAsync();
            var severity = list.Find(s => s.Id == id);

            return severity;
        }
    }
}
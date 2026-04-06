using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Repository;

namespace Wasel_Palestine.BAL.Service
{
    public class CheckpointStatusService : ICheckpointStatusService
    {
        private readonly ICheckpointStatusRepository _repo;
        private readonly IMemoryCache _cache;
        private const string StatusCacheKey = "CheckpointStatuses_All";

        public CheckpointStatusService(ICheckpointStatusRepository repo, IMemoryCache cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task<List<CheckpointStatusResponse>> GetAllAsync()
        {
            if (!_cache.TryGetValue(StatusCacheKey, out List<CheckpointStatusResponse> cachedStatuses))
            {
                var statuses = await _repo.GetAllAsync();

                cachedStatuses = statuses.Select(s => new CheckpointStatusResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Success = true
                }).ToList();

                _cache.Set(StatusCacheKey, cachedStatuses, TimeSpan.FromHours(24));
            }

            return cachedStatuses;
        }

        public async Task<CheckpointStatusResponse> CreateAsync(CreateCheckpointStatusRequest request)
        {
            var response = new CheckpointStatusResponse();
          
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return new CheckpointStatusResponse
                    {
                        Success = false,
                        Message = "Status name is required"
                    };
                }

                var exists = await _repo.ExistsAsync(request.Name);
                if (exists)
                {
                    return new CheckpointStatusResponse
                    {
                        Success = false,
                        Message = "Status already exists"
                    };
                }

                var status = new CheckpointStatus
                {
                    Name = request.Name.Trim(),
                    Description = request.Description
                };

                var created = await _repo.CreateAsync(status);

                _cache.Remove(StatusCacheKey);

                return new CheckpointStatusResponse
                {
                    Id = created.Id,
                    Name = created.Name,
                    Description = created.Description,
                    Success = true,
                    Message = "Status created successfully"
                };
            }
            catch (Exception ex)
            {
                return new CheckpointStatusResponse
                {
                    Success = false,
                    Message = "Error occurred",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
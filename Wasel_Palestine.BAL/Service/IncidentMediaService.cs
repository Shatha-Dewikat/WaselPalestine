using Mapster;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;

namespace Wasel_Palestine.BLL.Service
{
    public class IncidentMediaService : IIncidentMediaService
    {
        private readonly IIncidentMediaRepository _repository;
        private readonly IFileService _fileService;
        private readonly IMemoryCache _cache;
        private const string MediaCacheKey = "IncidentMedia_";

        public IncidentMediaService(IIncidentMediaRepository repository, IFileService fileService, IMemoryCache cache)
        {
            _repository = repository;
            _fileService = fileService;
            _cache = cache;
        }

        private void ClearMediaCache(int incidentId)
        {
            _cache.Remove($"{MediaCacheKey}{incidentId}");
        }

        public async Task<IncidentMediaResponse> AddMediaAsync(IncidentMediaCreateRequest request)
        {
            var file = request.File;
            var fileName = await _fileService.UploadAsync(request.File);
            if (fileName == null)
                throw new Exception("No file uploaded");

            var baseUrl = "http://localhost:5034";
            var media = new IncidentMedia
            {
                IncidentId = request.IncidentId,
                FileName = fileName,
                Url = $"{baseUrl}/images/{fileName}",
                ContentType = file.ContentType,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _repository.AddAsync(media);

            ClearMediaCache(request.IncidentId);

            return result.Adapt<IncidentMediaResponse>();
        }

        public async Task DeleteMediaAsync(int id)
        {
            var media = await _repository.GetByIdAsync(id);
            if (media == null)
                throw new KeyNotFoundException("Media not found");

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", media.FileName);
            if (File.Exists(path))
                File.Delete(path);

            await _repository.DeleteAsync(media);

            ClearMediaCache(media.IncidentId);
        }

        public async Task<List<IncidentMediaResponse>> GetByIncidentIdAsync(int incidentId)
        {
            string cacheKey = $"{MediaCacheKey}{incidentId}";

            if (!_cache.TryGetValue(cacheKey, out List<IncidentMediaResponse> cachedMedia))
            {
                var list = await _repository.GetByIncidentIdAsync(incidentId);
                cachedMedia = list.Adapt<List<IncidentMediaResponse>>();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(cacheKey, cachedMedia, cacheOptions);
            }

            return cachedMedia;
        }
    }
}
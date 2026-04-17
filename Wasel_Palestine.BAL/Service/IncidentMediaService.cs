using Mapster;
using Microsoft.Extensions.Caching.Memory;
using Wasel_Palestine.BAL.Service;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Repository;

namespace Wasel_Palestine.BAL.Service
{
public class IncidentMediaService : IIncidentMediaService
{
    private readonly IIncidentMediaRepository _repository;
    private readonly IFileService _fileService;
    private readonly IMemoryCache _cache;
    private const string MediaCacheKey = "IncidentMedia_";
    private const string IncidentCacheKey = "Incident_"; 

    public IncidentMediaService(IIncidentMediaRepository repository, IFileService fileService, IMemoryCache cache)
    {
        _repository = repository;
        _fileService = fileService;
        _cache = cache;
    }

    public async Task<IncidentMediaResponse> AddMediaAsync(IncidentMediaCreateRequest request)
    {
        var fileName = await _fileService.UploadAsync(request.File);
        if (fileName == null) throw new Exception("No file uploaded");

       
        var media = new IncidentMedia
        {
            IncidentId = request.IncidentId,
            FileName = fileName,
            Url = $"/images/{fileName}",
            ContentType = request.File.ContentType,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _repository.AddAsync(media);

       
        _cache.Remove($"{MediaCacheKey}{request.IncidentId}");
        _cache.Remove($"{IncidentCacheKey}{request.IncidentId}");

        
        var baseUrl = "http://localhost:5034";
        var response = result.Adapt<IncidentMediaResponse>();
        response.Url = $"{baseUrl}{result.Url}";

        return response;
    }

    public async Task<List<IncidentMediaResponse>> GetByIncidentIdAsync(int incidentId)
    {
        string cacheKey = $"{MediaCacheKey}{incidentId}";
        var baseUrl = "http://localhost:5034";

        if (!_cache.TryGetValue(cacheKey, out List<IncidentMediaResponse> cachedMedia))
        {
            var list = await _repository.GetByIncidentIdAsync(incidentId);
            cachedMedia = list.Select(m => {
                var res = m.Adapt<IncidentMediaResponse>();
                res.Url = m.Url.StartsWith("http") ? m.Url : $"{baseUrl}{m.Url}";
                return res;
            }).ToList();

            _cache.Set(cacheKey, cachedMedia, TimeSpan.FromMinutes(10));
        }
        return cachedMedia;
    }

    public async Task DeleteMediaAsync(int id)
    {
        var media = await _repository.GetByIdAsync(id);
        if (media == null) throw new KeyNotFoundException("Media not found");

        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", media.FileName);
        if (File.Exists(path)) File.Delete(path);

        await _repository.DeleteAsync(media);
        _cache.Remove($"{MediaCacheKey}{media.IncidentId}");
        _cache.Remove($"{IncidentCacheKey}{media.IncidentId}");
    }
}
}
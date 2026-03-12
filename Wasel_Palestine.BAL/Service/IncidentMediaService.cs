using Mapster;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Repository;

namespace Wasel_Palestine.BLL.Service
{
    public class IncidentMediaService : IIncidentMediaService
    {
        private readonly IIncidentMediaRepository _repository;
        private readonly IFileService _fileService;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IncidentMediaService(IIncidentMediaRepository repository, IFileService fileService, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _fileService = fileService;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetCurrentUserId() => _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;

        private Task LogAuditAsync(string action, string entityName, int entityId, string details)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = GetCurrentUserId(),
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Timestamp = DateTime.UtcNow,
                Details = details,
                IPAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
                UserAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown"
            });
            return Task.CompletedTask;
        }

        public async Task<IncidentMediaResponse> AddMediaAsync(IncidentMediaCreateRequest request)
        {
            var fileName = await _fileService.UploadAsync(request.File);
            if (fileName == null) throw new Exception("No file uploaded");

            var media = new IncidentMedia
            {
                IncidentId = request.IncidentId,
                Url = $"/images/{fileName}",
                CreatedAt = DateTime.UtcNow
            };

            var result = await _repository.AddAsync(media);
            await LogAuditAsync("Create", nameof(IncidentMedia), result.Id, $"Added media '{fileName}' to incident {media.IncidentId}");
            await _context.SaveChangesAsync();

            return result.Adapt<IncidentMediaResponse>();
        }

        public async Task DeleteMediaAsync(int id)
        {
            var media = await _repository.GetByIdAsync(id);
            if (media == null) throw new KeyNotFoundException("Media not found");

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", media.Url.TrimStart('/'));
            if (File.Exists(path)) File.Delete(path);

            await _repository.DeleteAsync(media);
            await LogAuditAsync("Delete", nameof(IncidentMedia), media.Id, $"Deleted media from incident {media.IncidentId}");
            await _context.SaveChangesAsync();
        }

        public async Task<List<IncidentMediaResponse>> GetByIncidentIdAsync(int incidentId)
        {
            var list = await _repository.GetByIncidentIdAsync(incidentId);
            return list.Adapt<List<IncidentMediaResponse>>();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Repository;

namespace Wasel_Palestine.BLL.Service
{
    public class IncidentService : IIncidentService
    {
        private readonly IIncidentRepository _incidentRepo;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IncidentService(
            IIncidentRepository incidentRepo,
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _incidentRepo = incidentRepo;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
        }

        private Task LogAuditAsync(string action, string entityName, int entityId, string details)
        {
            var audit = new AuditLog
            {
                UserId = GetCurrentUserId(),
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Timestamp = DateTime.UtcNow,
                Details = details,
                IPAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
                UserAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown"
            };

            _context.AuditLogs.Add(audit);

            return Task.CompletedTask;
        }

        public async Task<IncidentResponse> CreateIncidentAsync(CreateIncidentRequest request, string userId = null)
        {
           
            userId ??= GetCurrentUserId();

           
            var location = new Location
            {
                Latitude = (decimal)request.Latitude,
                Longitude = (decimal)request.Longitude,
                AreaName = request.AreaName,
                City = request.City,
                CreatedAt = DateTime.UtcNow
            };

           
            var incident = new Incident
            {
                Title = request.Title,
                TitleAr = request.TitleAr,
                Description = request.Description,
                DescriptionAr = request.DescriptionAr,
                CategoryId = request.CategoryId,
                SeverityId = request.SeverityId,
                StatusId = 1, 
                Location = location,
                CheckpointId = request.CheckpointId,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

        
            await _incidentRepo.AddAsync(incident);

            _context.IncidentHistories.Add(new IncidentHistory
            {
                IncidentId = incident.Id,
                StatusId = incident.StatusId,
                ChangedByUserId = userId,
                ChangedAt = DateTime.UtcNow
            });

           
            await LogAuditAsync("Create", nameof(Incident), incident.Id, $"Created incident '{incident.Title}'");

            await _context.SaveChangesAsync();

            return new IncidentResponse
            {
                Id = incident.Id,
                Title = incident.Title,
                Description = incident.Description,
                Category = incident.Category?.Name,
                Severity = incident.Severity?.Name,
                Status = incident.Status?.Name,
                Verified = false,
                Latitude = (double)location.Latitude,
                Longitude = (double)location.Longitude,
                CreatedAt = incident.CreatedAt
            };
        }

        public async Task<IncidentResponse> UpdateIncidentAsync(int id, UpdateIncidentRequest request, string userId = null)
        {
            userId ??= GetCurrentUserId();

            var incident = await _incidentRepo.GetByIdAsync(id);

            if (incident == null)
                return new IncidentResponse { Success = false, Message = "Incident not found" };

            var oldStatusId = incident.StatusId;

            var oldData = $"Title:{incident.Title}, Description:{incident.Description}, Status:{incident.StatusId}";

            incident.Title = string.IsNullOrEmpty(request.Title) ? incident.Title : request.Title;
            incident.TitleAr = string.IsNullOrEmpty(request.TitleAr) ? incident.TitleAr : request.TitleAr;
            incident.Description = string.IsNullOrEmpty(request.Description) ? incident.Description : request.Description;
            incident.DescriptionAr = string.IsNullOrEmpty(request.DescriptionAr) ? incident.DescriptionAr : request.DescriptionAr;

            incident.CategoryId = request.CategoryId ?? incident.CategoryId;
            incident.SeverityId = request.SeverityId ?? incident.SeverityId;
            incident.StatusId = request.StatusId ?? incident.StatusId;

            if (incident.Location == null)
                incident.Location = new Location();

            incident.Location.Latitude = request.Latitude.HasValue ? (decimal)request.Latitude.Value : incident.Location.Latitude;
            incident.Location.Longitude = request.Longitude.HasValue ? (decimal)request.Longitude.Value : incident.Location.Longitude;

            await _incidentRepo.UpdateAsync(incident);

            if (request.StatusId.HasValue && request.StatusId.Value != oldStatusId)
            {
                _context.IncidentHistories.Add(new IncidentHistory
                {
                    IncidentId = incident.Id,
                    StatusId = request.StatusId.Value,
                    ChangedByUserId = userId,
                    ChangedAt = DateTime.UtcNow
                });
            }

            var newData = $"Title:{incident.Title}, Description:{incident.Description}, Status:{incident.StatusId}";

            await LogAuditAsync("Update", nameof(Incident), incident.Id,
                $"Updated incident. Before: {oldData}. After: {newData}");

            await _context.SaveChangesAsync();

            return new IncidentResponse
            {
                Id = incident.Id,
                Title = incident.Title,
                Description = incident.Description,
                Category = incident.Category?.Name,
                Severity = incident.Severity?.Name,
                Status = incident.Status?.Name,
                Latitude = (double)incident.Location.Latitude,
                Longitude = (double)incident.Location.Longitude,
                CreatedAt = incident.CreatedAt
            };
        }

        public async Task<IncidentResponse> DeleteIncidentAsync(int id, string userId = null)
        {
            userId ??= GetCurrentUserId();

            var incident = await _incidentRepo.GetByIdAsync(id);

            if (incident == null)
                return new IncidentResponse { Success = false, Message = "Incident not found" };

            await _incidentRepo.DeleteAsync(incident);

            await LogAuditAsync("Delete", nameof(Incident), incident.Id,
                $"Deleted incident '{incident.Title}'");

            await _context.SaveChangesAsync();

            return new IncidentResponse
            {
                Success = true,
                Message = "Incident deleted successfully"
            };
        }

        public async Task<IncidentResponse> GetIncidentByIdAsync(int id, string lang = "en")
        {
            var incident = await _incidentRepo.GetByIdAsync(id);

            if (incident == null)
                return new IncidentResponse { Success = false, Message = "Incident not found" };

            return new IncidentResponse
            {
                Id = incident.Id,
                Title = lang == "ar" ? incident.TitleAr : incident.Title,
                Description = lang == "ar" ? incident.DescriptionAr : incident.Description,
                Category = incident.Category?.Name,
                Severity = incident.Severity?.Name,
                Status = incident.Status?.Name,
                Latitude = (double)incident.Location.Latitude,
                Longitude = (double)incident.Location.Longitude,
                CreatedAt = incident.CreatedAt
            };
        }

        public async Task<List<IncidentResponse>> GetIncidentAllAsync(string lang = "en")
        {
            var incidents = await _incidentRepo.GetAllAsync();

            return incidents.Select(i => new IncidentResponse
            {
                Id = i.Id,
                Title = lang == "ar" ? i.TitleAr : i.Title,
                Description = lang == "ar" ? i.DescriptionAr : i.Description,
                Category = i.Category?.Name,
                Severity = i.Severity?.Name,
                Status = i.Status?.Name,
                Latitude = (double)i.Location.Latitude,
                Longitude = (double)i.Location.Longitude,
                CreatedAt = i.CreatedAt
            }).ToList();
        }

        public async Task<List<IncidentHistoryResponse>> GetIncidentHistoryAsync(int incidentId)
        {
            return await _context.IncidentHistories
                .Include(h => h.Status)
                .Where(h => h.IncidentId == incidentId)
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => new IncidentHistoryResponse
                {
                    StatusId = h.StatusId,
                    Status = h.Status.Name,
                    ChangedByUserId = h.ChangedByUserId,
                    ChangedAt = h.ChangedAt
                })
                .ToListAsync();
        }

        public async Task<List<IncidentResponse>> GetPagedIncidentsAsync(PaginationRequest paginationRequest, string lang = "en")
        {
            var incidents = await _incidentRepo.GetPagedAsync(paginationRequest);

            return incidents.Select(i => new IncidentResponse
            {
                Id = i.Id,
                Title = lang == "ar" ? i.TitleAr : i.Title,
                Description = lang == "ar" ? i.DescriptionAr : i.Description,
                Category = i.Category?.Name,
                Severity = i.Severity?.Name,
                Status = i.Status?.Name,
                Latitude = (double)i.Location.Latitude,
                Longitude = (double)i.Location.Longitude,
                CreatedAt = i.CreatedAt
            }).ToList();
        }

        public async Task<List<IncidentResponse>> GetFilteredIncidentsAsync(IncidentFilterRequest filter, string lang = "en")
        {
            var incidents = await _incidentRepo.GetFilteredAsync(filter);

            return incidents.Select(i => new IncidentResponse
            {
                Id = i.Id,
                Title = lang == "ar" ? i.TitleAr : i.Title,
                Description = lang == "ar" ? i.DescriptionAr : i.Description,
                Category = i.Category?.Name,
                Severity = i.Severity?.Name,
                Status = i.Status?.Name,
                Latitude = (double)i.Location.Latitude,
                Longitude = (double)i.Location.Longitude,
                CreatedAt = i.CreatedAt
            }).ToList();
        }

        public async Task<List<IncidentResponse>> GetFilteredPagedIncidentsAsync(IncidentQueryRequest request, string lang = "en")
        {
            var incidents = await _incidentRepo.GetFilteredPagedAsync(request);

            return incidents.Select(i => new IncidentResponse
            {
                Id = i.Id,
                Title = lang == "ar" ? i.TitleAr : i.Title,
                Description = lang == "ar" ? i.DescriptionAr : i.Description,
                Category = i.Category?.Name,
                Severity = i.Severity?.Name,
                Status = i.Status?.Name,
                Latitude = (double)i.Location.Latitude,
                Longitude = (double)i.Location.Longitude,
                CreatedAt = i.CreatedAt
            }).ToList();
        }
    }
}
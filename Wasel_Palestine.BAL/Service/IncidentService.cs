using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Repository;
using Microsoft.EntityFrameworkCore;

namespace Wasel_Palestine.BLL.Service
{
    public class IncidentService : IIncidentService
    {
        private readonly IIncidentRepository _incidentRepo;
        private readonly ApplicationDbContext _context;

        public IncidentService(IIncidentRepository incidentRepo, ApplicationDbContext context)
        {
            _incidentRepo = incidentRepo;
            _context = context;
        }

        private async Task LogAuditAsync(string userId, string action, string entityName, int entityId, string details)
        {
            var audit = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Timestamp = DateTime.UtcNow,
                Details = details,
                IPAddress = "TODO",
                UserAgent = "TODO"
            };
            _context.AuditLogs.Add(audit);
            await _context.SaveChangesAsync();
        }

        public async Task<IncidentResponse> CreateIncidentAsync(CreateIncidentRequest request, string userId)
        {
            var location = new Location
            {
                Latitude = (decimal)request.Latitude,
                Longitude = (decimal)request.Longitude
            };

            var incident = new Incident
            {
                Title = request.Title,
                Description = request.Description,
                CategoryId = request.CategoryId,
                SeverityId = request.SeverityId,
                StatusId = 1,
                Location = location,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _incidentRepo.AddAsync(incident);

            await LogAuditAsync(userId, "Create", nameof(Incident), incident.Id, $"Created incident '{incident.Title}'");

            return new IncidentResponse
            {
                Id = incident.Id,
                Title = incident.Title,
                Description = incident.Description,
                Category = incident.Category?.Name,
                Severity = incident.Severity?.Name,
                Status = incident.Status?.Name,
                Latitude = (double)location.Latitude,
                Longitude = (double)location.Longitude,
                CreatedAt = incident.CreatedAt
            };
        }

        public async Task<IncidentResponse> UpdateIncidentAsync(int id, UpdateIncidentRequest request, string userId)
        {
            var incident = await _incidentRepo.GetByIdAsync(id);
            if (incident == null)
                return new IncidentResponse { Success = false, Message = "Incident not found" };

            var oldData = $"Title:{incident.Title}, Description:{incident.Description}, Status:{incident.StatusId}";

            incident.Title = string.IsNullOrEmpty(request.Title) ? incident.Title : request.Title;
            incident.Description = string.IsNullOrEmpty(request.Description) ? incident.Description : request.Description;
            incident.CategoryId = request.CategoryId ?? incident.CategoryId;
            incident.SeverityId = request.SeverityId ?? incident.SeverityId;
            incident.StatusId = request.StatusId ?? incident.StatusId;

            if (incident.Location == null) incident.Location = new Location();
            incident.Location.Latitude = request.Latitude.HasValue ? (decimal)request.Latitude.Value : incident.Location.Latitude;
            incident.Location.Longitude = request.Longitude.HasValue ? (decimal)request.Longitude.Value : incident.Location.Longitude;

            await _incidentRepo.UpdateAsync(incident);

            var newData = $"Title:{incident.Title}, Description:{incident.Description}, Status:{incident.StatusId}";
            await LogAuditAsync(userId, "Update", nameof(Incident), incident.Id, $"Updated incident. Before: {oldData}. After: {newData}");

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

        public async Task<IncidentResponse> DeleteIncidentAsync(int id, string userId)
        {
            var incident = await _incidentRepo.GetByIdAsync(id);
            if (incident == null)
                return new IncidentResponse { Success = false, Message = "Incident not found" };

            await _incidentRepo.DeleteAsync(incident);

            await LogAuditAsync(userId, "Delete", nameof(Incident), incident.Id, $"Deleted incident '{incident.Title}'");

            return new IncidentResponse { Success = true, Message = "Incident deleted successfully" };
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
            var response = new List<IncidentResponse>();
            foreach (var incident in incidents)
            {
                response.Add(new IncidentResponse
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
                });
            }
            return response;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
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

        public IncidentService(IIncidentRepository incidentRepo, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _incidentRepo = incidentRepo;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetCurrentUserId() => _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
        private string GetIP() => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        private string GetUserAgent() => _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";

        private async Task LogAuditAsync(string action, string entityName, int entityId, string details)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = GetCurrentUserId(),
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Timestamp = DateTime.UtcNow,
                Details = details,
                IPAddress = GetIP(),
                UserAgent = GetUserAgent()
            });
            await Task.CompletedTask;
        }

        #region Create / Update / Delete
        public async Task<IncidentResponse> CreateIncidentAsync(CreateIncidentRequest request, string userId = null)
        {
            userId ??= GetCurrentUserId();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                
                var exists = await _context.Incidents.AnyAsync(i => i.Title == request.Title);
                if (exists)
                {
                    return new IncidentResponse
                    {
                        Success = false,
                        Message = "Incident with the same title already exists",
                        Errors = new List<string> { "Duplicate incident title" }
                    };
                }

                var location = new Location
                {
                    Latitude = (decimal)request.Latitude,
                    Longitude = (decimal)request.Longitude,
                    AreaName = request.AreaName,
                    City = request.City,
                    CreatedAt = DateTime.UtcNow
                };
                var openStatus = await _context.IncidentStatuses
                .FirstOrDefaultAsync(s => s.Name == "Open");
                var incident = new Incident
                {
                    Title = request.Title,
                    TitleAr = request.TitleAr,
                    Description = request.Description,
                    DescriptionAr = request.DescriptionAr,
                    CategoryId = request.CategoryId,
                    SeverityId = request.SeverityId,
                    StatusId = openStatus.Id, // Open
                    Location = location,
                    CheckpointId = request.CheckpointId,

                    CreatedByUserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _incidentRepo.AddAsync(incident);

                await _context.Entry(incident).Reference(i => i.Category).LoadAsync();
                await _context.Entry(incident).Reference(i => i.Severity).LoadAsync();
                await _context.Entry(incident).Reference(i => i.Status).LoadAsync();

                _context.IncidentHistories.Add(new IncidentHistory
                {
                    IncidentId = incident.Id,
                    StatusId = incident.StatusId,
                    ChangedByUserId = userId,
                    ChangedAt = DateTime.UtcNow,
                    Action = "Created",
                    Changes = $"Title:{incident.Title}, Description:{incident.Description}, Status:{incident.StatusId}"
                });

                await LogAuditAsync("Create", nameof(Incident), incident.Id, $"Created incident '{incident.Title}'");

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return MapToResponse(incident);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new IncidentResponse
                {
                    Success = false,
                    Message = "Failed to create incident",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<IncidentResponse> UpdateIncidentAsync(int id, UpdateIncidentRequest request, string userId = null)
        {
            userId ??= GetCurrentUserId();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var incident = await _context.Incidents
                    .Include(i => i.Category)
                    .Include(i => i.Severity)
                    .Include(i => i.Status)
                    .Include(i => i.IncidentMedia)
                    .Include(i => i.Location)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (incident == null)
                    return new IncidentResponse
                    {
                        Success = false,
                        Message = "Incident not found",
                        Errors = new List<string> { "Invalid incident ID" }
                    };

                var oldData = $"Title:{incident.Title}, Description:{incident.Description}, Status:{incident.StatusId}";

                incident.Title = string.IsNullOrEmpty(request.Title) ? incident.Title : request.Title;
                incident.TitleAr = string.IsNullOrEmpty(request.TitleAr) ? incident.TitleAr : request.TitleAr;
                incident.Description = string.IsNullOrEmpty(request.Description) ? incident.Description : request.Description;
                incident.DescriptionAr = string.IsNullOrEmpty(request.DescriptionAr) ? incident.DescriptionAr : request.DescriptionAr;
                incident.CategoryId = request.CategoryId ?? incident.CategoryId;
                incident.SeverityId = request.SeverityId ?? incident.SeverityId;
                incident.StatusId = request.StatusId ?? incident.StatusId;

                if (incident.Location == null) incident.Location = new Location();
                incident.Location.Latitude = request.Latitude.HasValue ? (decimal)request.Latitude.Value : incident.Location.Latitude;
                incident.Location.Longitude = request.Longitude.HasValue ? (decimal)request.Longitude.Value : incident.Location.Longitude;

                await _incidentRepo.UpdateAsync(incident);

                await _context.Entry(incident).Reference(i => i.Category).LoadAsync();
                await _context.Entry(incident).Reference(i => i.Severity).LoadAsync();
                await _context.Entry(incident).Reference(i => i.Status).LoadAsync();

                _context.IncidentHistories.Add(new IncidentHistory
                {
                    IncidentId = incident.Id,
                    StatusId = incident.StatusId,
                    ChangedByUserId = userId,
                    ChangedAt = DateTime.UtcNow,
                    Action = "Updated",
                    Changes = $"Before: {oldData}. After: Title:{incident.Title}, Description:{incident.Description}, Status:{incident.StatusId}"
                });

                await LogAuditAsync("Update", nameof(Incident), incident.Id, $"Before: {oldData}. After: Title:{incident.Title}, Description:{incident.Description}, Status:{incident.StatusId}");

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return MapToResponse(incident);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new IncidentResponse
                {
                    Success = false,
                    Message = "Failed to update incident",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<IncidentResponse> DeleteIncidentAsync(int id, string userId = null)
        {
            userId ??= GetCurrentUserId();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var incident = await _context.Incidents
                    .Include(i => i.Category)
                    .Include(i => i.Severity)
                    .Include(i => i.Status)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (incident == null)
                    return new IncidentResponse
                    {
                        Success = false,
                        Message = "Incident not found",
                        Errors = new List<string> { "Invalid incident ID" }
                    };

                var oldData = $"Title:{incident.Title}, Description:{incident.Description}, Status:{incident.StatusId}";

                await _incidentRepo.DeleteAsync(incident);

                _context.IncidentHistories.Add(new IncidentHistory
                {
                    IncidentId = incident.Id,
                    StatusId = incident.StatusId,
                    ChangedByUserId = userId,
                    ChangedAt = DateTime.UtcNow,
                    Action = "Deleted",
                    Changes = $"Before: {oldData}"
                });

                await LogAuditAsync("Delete", nameof(Incident), incident.Id, $"Deleted incident '{incident.Title}'");

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new IncidentResponse
                {
                    Success = true,
                    Message = "Incident deleted successfully",
                    Errors = new List<string>()
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new IncidentResponse
                {
                    Success = false,
                    Message = "Failed to delete incident",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
        #endregion

        #region Get / GetAll / History
        public async Task<IncidentResponse> GetIncidentByIdAsync(int id, string lang = "en")
        {
            var incident = await _context.Incidents
                .Include(i => i.Category)
                .Include(i => i.Severity)
                .Include(i => i.Status)
                .Include(i => i.IncidentMedia)
                .Include(i => i.Location)
                .Include(i => i.IncidentHistories)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (incident == null)
                return new IncidentResponse
                {
                    Success = false,
                    Message = "Incident not found",
                    Errors = new List<string> { "Invalid incident ID" }
                };

            return MapToResponse(incident, lang);
        }

        public async Task<List<IncidentResponse>> GetIncidentAllAsync(string lang = "en")
        {
            var incidents = await _context.Incidents
                .Include(i => i.Category)
                .Include(i => i.Severity)
                .Include(i => i.Status)
                .Include(i => i.IncidentMedia)
                .Include(i => i.Location)
                .Include(i => i.IncidentHistories)
                .ToListAsync();

            return incidents.Select(i => MapToResponse(i, lang)).ToList();
        }

        public async Task<List<IncidentHistoryResponse>> GetIncidentHistoryAsync(int incidentId)
        {
            var histories = await _context.IncidentHistories
                .Where(h => h.IncidentId == incidentId)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();

            return histories.Select(h => new IncidentHistoryResponse
            {
                IncidentId = h.IncidentId,
                StatusId = h.StatusId,
                Action = h.Action,
                Changes = h.Changes,
                ChangedByUserId = h.ChangedByUserId,
                ChangedAt = h.ChangedAt
            }).ToList();
        }
        #endregion

        public async Task<SimpleResponse> VerifyIncidentAsync(int incidentId, string userId)
        {
            try
            {
                var incident = await _context.Incidents.FindAsync(incidentId);

                if (incident == null)
                    return new SimpleResponse { Success = false, Message = "Incident not found", Errors = new List<string> { "Invalid incident ID" } };

                var verifiedStatus = await _context.IncidentStatuses.FirstOrDefaultAsync(s => s.Name == "Verified");

                if (verifiedStatus == null)
                    return new SimpleResponse { Success = false, Message = "Verified status not found", Errors = new List<string> { "Verified status not configured in DB" } };

                if (incident.StatusId == verifiedStatus.Id)
                    return new SimpleResponse { Success = false, Message = "Incident already verified", Errors = new List<string> { "Duplicate verification attempt" } };

                incident.StatusId = verifiedStatus.Id;
                incident.Verified = true;
                incident.VerifiedAt = DateTime.UtcNow;

                // سجل في IncidentHistory
                _context.IncidentHistories.Add(new IncidentHistory
                {
                    IncidentId = incident.Id,
                    StatusId = verifiedStatus.Id,
                    ChangedByUserId = userId,
                    ChangedAt = DateTime.UtcNow,
                    Action = "Verified",
                    Changes = "Incident verified"
                });

                // سجل في AuditLog
                await LogAuditAsync("Verify", nameof(Incident), incident.Id, $"Incident verified by user {userId}");

                await _context.SaveChangesAsync();

                return new SimpleResponse
                {
                    Success = true,
                    Message = "Incident verified successfully",
                    Errors = new List<string>()
                };
            }
            catch (Exception ex)
            {
                return new SimpleResponse
                {
                    Success = false,
                    Message = "Failed to verify incident",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<SimpleResponse> ResolveIncidentAsync(int incidentId, string userId)
        {
            try
            {
                var incident = await _context.Incidents.FindAsync(incidentId);

                if (incident == null)
                    return new SimpleResponse { Success = false, Message = "Incident not found", Errors = new List<string> { "Invalid incident ID" } };

                var resolvedStatus = await _context.IncidentStatuses.FirstOrDefaultAsync(s => s.Name == "Resolved");

                if (resolvedStatus == null)
                    return new SimpleResponse { Success = false, Message = "Resolved status not found", Errors = new List<string> { "Resolved status not configured in DB" } };

                if (incident.StatusId == resolvedStatus.Id)
                    return new SimpleResponse { Success = false, Message = "Incident already resolved", Errors = new List<string> { "Duplicate resolve attempt" } };

                incident.StatusId = resolvedStatus.Id;

                _context.IncidentHistories.Add(new IncidentHistory
                {
                    IncidentId = incident.Id,
                    StatusId = resolvedStatus.Id,
                    ChangedByUserId = userId,
                    ChangedAt = DateTime.UtcNow,
                    Action = "Resolved",
                    Changes = "Incident resolved"
                });

                await LogAuditAsync("Resolve", nameof(Incident), incident.Id, $"Incident resolved by user {userId}");

                await _context.SaveChangesAsync();

                return new SimpleResponse
                {
                    Success = true,
                    Message = "Incident resolved successfully",
                    Errors = new List<string>()
                };
            }
            catch (Exception ex)
            {
                return new SimpleResponse
                {
                    Success = false,
                    Message = "Failed to resolve incident",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<SimpleResponse> CloseIncidentAsync(int incidentId, string userId)
        {
            try
            {
                var incident = await _context.Incidents.FindAsync(incidentId);

                if (incident == null)
                    return new SimpleResponse { Success = false, Message = "Incident not found", Errors = new List<string> { "Invalid incident ID" } };

                var closedStatus = await _context.IncidentStatuses.FirstOrDefaultAsync(s => s.Name == "Closed");

                if (closedStatus == null)
                    return new SimpleResponse { Success = false, Message = "Closed status not found", Errors = new List<string> { "Closed status not configured in DB" } };

                if (incident.StatusId == closedStatus.Id)
                    return new SimpleResponse { Success = false, Message = "Incident already closed", Errors = new List<string> { "Duplicate close attempt" } };

                incident.StatusId = closedStatus.Id;

                _context.IncidentHistories.Add(new IncidentHistory
                {
                    IncidentId = incident.Id,
                    StatusId = closedStatus.Id,
                    ChangedByUserId = userId,
                    ChangedAt = DateTime.UtcNow,
                    Action = "Closed",
                    Changes = "Incident closed"
                });

                await LogAuditAsync("Close", nameof(Incident), incident.Id, $"Incident closed by user {userId}");

                await _context.SaveChangesAsync();

                return new SimpleResponse
                {
                    Success = true,
                    Message = "Incident closed successfully",
                    Errors = new List<string>()
                };
            }
            catch (Exception ex)
            {
                return new SimpleResponse
                {
                    Success = false,
                    Message = "Failed to close incident",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
        #region Filtering / Pagination
        public async Task<List<IncidentResponse>> GetFilteredIncidentsAsync(IncidentFilterRequest filter, string lang = "en")
        {
            var query = _context.Incidents
                .Include(i => i.Category)
                .Include(i => i.Severity)
                .Include(i => i.IncidentMedia)
                .Include(i => i.Status)
                .Include(i => i.Location)
                .AsQueryable();

            if (filter.CategoryId.HasValue)
                query = query.Where(i => i.CategoryId == filter.CategoryId.Value);

            if (filter.SeverityId.HasValue)
                query = query.Where(i => i.SeverityId == filter.SeverityId.Value);

            if (filter.StatusId.HasValue)
                query = query.Where(i => i.StatusId == filter.StatusId.Value);

            if (filter.DateFrom.HasValue)
                query = query.Where(i => i.CreatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(i => i.CreatedAt <= filter.DateTo.Value);

            var incidents = await query.ToListAsync();
            return incidents.Select(i => MapToResponse(i, lang)).ToList();
        }

        public async Task<List<IncidentResponse>> GetPagedIncidentsAsync(PaginationRequest pagination, string lang = "en")
        {
            var query = _context.Incidents
                .Include(i => i.Category)
                .Include(i => i.Severity)
                .Include(i => i.IncidentMedia)
                .Include(i => i.Status)
                .Include(i => i.Location)
                .AsQueryable();

            var incidents = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return incidents.Select(i => MapToResponse(i, lang)).ToList();
        }

        public async Task<List<IncidentResponse>> GetFilteredPagedIncidentsAsync(IncidentQueryRequest request, string lang = "en")
        {
            var query = _context.Incidents
                .Include(i => i.Category)
                .Include(i => i.Severity)
                .Include(i => i.IncidentMedia)
                .Include(i => i.Status)
                .Include(i => i.Location)
                .AsQueryable();

            var filter = request.Filter;

            if (filter.CategoryId.HasValue)
                query = query.Where(i => i.CategoryId == filter.CategoryId.Value);

            if (filter.SeverityId.HasValue)
                query = query.Where(i => i.SeverityId == filter.SeverityId.Value);

            if (filter.StatusId.HasValue)
                query = query.Where(i => i.StatusId == filter.StatusId.Value);

            if (filter.DateFrom.HasValue)
                query = query.Where(i => i.CreatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(i => i.CreatedAt <= filter.DateTo.Value);

            var incidents = await query
                .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .ToListAsync();

            return incidents.Select(i => MapToResponse(i, lang)).ToList();
        }
        #endregion

        #region Mapping
        private IncidentResponse MapToResponse(Incident incident, string lang = "en")
        {
            var baseUrl = "http://localhost:5034";
            return new IncidentResponse
            {
                Id = incident.Id,
                Title = lang == "ar" ? incident.TitleAr : incident.Title,
                Description = lang == "ar" ? incident.DescriptionAr : incident.Description,
                Category = incident.Category?.Name,
                Severity = incident.Severity?.Name,
                Status = incident.Status?.Name,
                Verified = incident.Verified,
                Latitude = incident.Location != null ? (double)incident.Location.Latitude : 0,
                Longitude = incident.Location != null ? (double)incident.Location.Longitude : 0,
                CreatedAt = incident.CreatedAt,

                History = incident.IncidentHistories?
                            .Select(h => new IncidentHistoryResponse
                            {
                                Id = h.Id,
                                StatusId = h.StatusId,
                                ChangedByUserId = h.ChangedByUserId,
                                ChangedAt = h.ChangedAt,
                                Action = h.Action,
                                Changes = h.Changes
                            }).ToList()
                            ?? new List<IncidentHistoryResponse>(),

                Media = incident.IncidentMedia?.Select(m => new IncidentMediaResponse
                {
                    Id = m.Id,
                    Url = $"{baseUrl}{m.Url}",
                    CreatedAt = m.CreatedAt
                }).ToList() ?? new List<IncidentMediaResponse>(),

                Success = true
            };

        }
        #endregion
    }
}
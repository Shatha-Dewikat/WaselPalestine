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
                    return new IncidentResponse { Success = false, Message = "Incident with the same title already exists" };
                }

                var openStatus = await _context.IncidentStatuses.FirstOrDefaultAsync(s => s.Name == "Open");
                if (openStatus == null) throw new Exception("Incident Status 'Open' not found in DB.");

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
                    StatusId = openStatus.Id,
                    Location = location,
                    CheckpointId = request.CheckpointId,
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _incidentRepo.AddAsync(incident);
                await _context.SaveChangesAsync();

               
                if (incident.CheckpointId.HasValue)
                {
                    var checkpoint = await _context.Checkpoints.FindAsync(incident.CheckpointId.Value);
                    var closedStatus = await _context.CheckpointStatuses.FirstOrDefaultAsync(s => s.Name == "Closed");

                    if (checkpoint != null && closedStatus != null)
                    {
                        string previousStatus = checkpoint.CurrentStatus;

                        checkpoint.CurrentStatus = "Closed";

                        _context.CheckpointStatusHistories.Add(new CheckpointStatusHistory
                        {
                            CheckpointId = checkpoint.Id,
                            OldStatus = previousStatus, 
                            NewStatus = "Closed",
                            ChangedAt = DateTime.UtcNow,
                            ChangedByUserId = userId
                        });
                    }
                }

                _context.IncidentHistories.Add(new IncidentHistory
                {
                    IncidentId = incident.Id,
                    StatusId = incident.StatusId,
                    ChangedByUserId = userId,
                    ChangedAt = DateTime.UtcNow,
                    Action = "Created",
                    Changes = $"Title:{incident.Title}, Status:{incident.StatusId}"
                });

                await LogAuditAsync("Create", nameof(Incident), incident.Id, $"Created incident '{incident.Title}'");
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return MapToResponse(incident);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new IncidentResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<IncidentResponse> UpdateIncidentAsync(int id, UpdateIncidentRequest request, string userId = null)
        {
            userId ??= GetCurrentUserId();
            var incident = await _context.Incidents.Include(i => i.Location).FirstOrDefaultAsync(i => i.Id == id);
            if (incident == null) return new IncidentResponse { Success = false, Message = "Incident not found" };

            incident.Title = request.Title ?? incident.Title;
            incident.TitleAr = request.TitleAr ?? incident.TitleAr;
            incident.Description = request.Description ?? incident.Description;
            incident.CategoryId = request.CategoryId ?? incident.CategoryId;
            incident.StatusId = request.StatusId ?? incident.StatusId;

            _context.IncidentHistories.Add(new IncidentHistory { IncidentId = id, StatusId = incident.StatusId, ChangedByUserId = userId, ChangedAt = DateTime.UtcNow, Action = "Updated" });

            await _context.SaveChangesAsync();
            return MapToResponse(incident);
        }

        public async Task<IncidentResponse> DeleteIncidentAsync(int id, string userId = null)
        {
            var incident = await _context.Incidents.FindAsync(id);
            if (incident == null) return new IncidentResponse { Success = false };
            await _incidentRepo.DeleteAsync(incident);
            await _context.SaveChangesAsync();
            return new IncidentResponse { Success = true };
        }
        #endregion

        #region Actions
        public async Task<SimpleResponse> VerifyIncidentAsync(int incidentId, string userId)
        {
            var incident = await _context.Incidents.FindAsync(incidentId);
            var status = await _context.IncidentStatuses.FirstOrDefaultAsync(s => s.Name == "Verified");
            if (incident == null || status == null) return new SimpleResponse { Success = false };

            incident.StatusId = status.Id;
            incident.Verified = true;
            incident.VerifiedAt = DateTime.UtcNow;

            _context.IncidentHistories.Add(new IncidentHistory { IncidentId = incidentId, StatusId = status.Id, ChangedByUserId = userId, ChangedAt = DateTime.UtcNow, Action = "Verified" });
            await _context.SaveChangesAsync();
            return new SimpleResponse { Success = true };
        }

        public async Task<SimpleResponse> ResolveIncidentAsync(int incidentId, string userId) => await UpdateStatusByName(incidentId, "Resolved", userId);
        public async Task<SimpleResponse> CloseIncidentAsync(int incidentId, string userId) => await UpdateStatusByName(incidentId, "Closed", userId);

        private async Task<SimpleResponse> UpdateStatusByName(int id, string statusName, string userId)
        {
            var incident = await _context.Incidents.FindAsync(id);
            var status = await _context.IncidentStatuses.FirstOrDefaultAsync(s => s.Name == statusName);
            if (incident == null || status == null) return new SimpleResponse { Success = false };

            incident.StatusId = status.Id;
            _context.IncidentHistories.Add(new IncidentHistory { IncidentId = id, StatusId = status.Id, ChangedByUserId = userId, ChangedAt = DateTime.UtcNow, Action = statusName });
            await _context.SaveChangesAsync();
            return new SimpleResponse { Success = true };
        }
        #endregion

        #region Queries
        public async Task<IncidentResponse> GetIncidentByIdAsync(int id, string lang = "en")
        {
            var incident = await _context.Incidents.Include(i => i.Category).Include(i => i.Status).Include(i => i.Location).Include(i => i.IncidentHistories).FirstOrDefaultAsync(i => i.Id == id);
            return incident == null ? new IncidentResponse { Success = false } : MapToResponse(incident, lang);
        }

        public async Task<List<IncidentResponse>> GetIncidentAllAsync(string lang = "en")
        {
            var incidents = await _context.Incidents.Include(i => i.Category).Include(i => i.Status).Include(i => i.Location).ToListAsync();
            return incidents.Select(i => MapToResponse(i, lang)).ToList();
        }

        public async Task<List<IncidentHistoryResponse>> GetIncidentHistoryAsync(int incidentId)
        {
            var histories = await _context.IncidentHistories.Where(h => h.IncidentId == incidentId).ToListAsync();
            return histories.Select(h => new IncidentHistoryResponse { IncidentId = h.IncidentId, StatusId = h.StatusId, Action = h.Action, ChangedAt = h.ChangedAt }).ToList();
        }

        public async Task<List<IncidentResponse>> GetFilteredIncidentsAsync(IncidentFilterRequest filter, string lang = "en")
        {
            var query = _context.Incidents.Include(i => i.Location).AsQueryable();
            if (filter.StatusId.HasValue) query = query.Where(i => i.StatusId == filter.StatusId);
            var data = await query.ToListAsync();
            return data.Select(i => MapToResponse(i, lang)).ToList();
        }

        public async Task<List<IncidentResponse>> GetPagedIncidentsAsync(PaginationRequest pagination, string lang = "en")
        {
            var data = await _context.Incidents.Include(i => i.Location).Skip((pagination.PageNumber - 1) * pagination.PageSize).Take(pagination.PageSize).ToListAsync();
            return data.Select(i => MapToResponse(i, lang)).ToList();
        }

        public async Task<List<IncidentResponse>> GetFilteredPagedIncidentsAsync(IncidentQueryRequest request, string lang = "en")
        {
            var query = _context.Incidents.Include(i => i.Location).AsQueryable();
            var data = await query.Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize).Take(request.Pagination.PageSize).ToListAsync();
            return data.Select(i => MapToResponse(i, lang)).ToList();
        }
        #endregion

        private IncidentResponse MapToResponse(Incident incident, string lang = "en")
        {
            return new IncidentResponse
            {
                Id = incident.Id,
                Title = lang == "ar" ? incident.TitleAr : incident.Title,
                Success = true,
                CreatedAt = incident.CreatedAt,
                Status = incident.Status?.Name,
                Category = incident.Category?.Name
            };
        }

        public async Task<List<IncidentResponse>> GetIncidentsByCheckpointIdAsync(int checkpointId, string lang = "en")
        {
            var incidents = await _context.Incidents
                .Include(i => i.Category)
                .Include(i => i.Severity)
                .Include(i => i.Status)
                .Include(i => i.Location)
                .Include(i => i.IncidentMedia)
                .Where(i => i.CheckpointId == checkpointId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return incidents.Select(i => MapToResponse(i, lang)).ToList();
        }
    }
}
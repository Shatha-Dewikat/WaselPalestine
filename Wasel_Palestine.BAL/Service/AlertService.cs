using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.BAL.Service
{
    public class AlertService : IAlertService
    {
        private readonly ApplicationDbContext _context;
        private const string SYSTEM_USER_ID = "SYSTEM_USER_ID_FROM_DB";

        public AlertService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<string> GetValidUserIdAsync(string userId)
        {
            if (!string.IsNullOrEmpty(userId) && await _context.Users.AnyAsync(u => u.Id == userId))
                return userId;

        
            var fallbackUser = await _context.Users.Select(u => u.Id).FirstOrDefaultAsync();

            return fallbackUser; 
        }

        public async Task<AlertResponse> CreateAlertAsync(AlertCreateRequest request, string userId, string ip, string userAgent)
        {
            var actualUserId = await GetValidUserIdAsync(userId);
            var alert = new Alert
            {
                IncidentId = request.IncidentId,
                CreatedAt = DateTime.UtcNow
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Alerts.AddAsync(alert);
                await _context.SaveChangesAsync();

                await _context.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = actualUserId,
                    Action = "Create",
                    EntityName = nameof(Alert),
                    EntityId = alert.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Created alert for IncidentId: {alert.IncidentId}",
                    IPAddress = ip,
                    UserAgent = userAgent
                });

                await _context.AlertHistories.AddAsync(new AlertHistory
                {
                    AlertId = alert.Id,
                    Status = "Created",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return new AlertResponse { Id = alert.Id, IncidentId = alert.IncidentId, CreatedAt = alert.CreatedAt, Success = true };
        }
        public async Task<List<AlertHistory>> GetAlertHistoryAsync(int alertId)
        {
            return await _context.AlertHistories
                .Where(h => h.AlertId == alertId)
                .OrderByDescending(h => h.Timestamp)
                .ToListAsync();
        }
        public async Task<AlertResponse> UpdateAlertAsync(AlertUpdateRequest request, string userId, string ip, string userAgent)
        {
            var actualUserId = await GetValidUserIdAsync(userId);
            var alert = await _context.Alerts.FindAsync(request.Id);
            if (alert == null) throw new KeyNotFoundException("Alert not found.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                alert.IncidentId = request.IncidentId;

                await _context.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = actualUserId,
                    Action = "Update",
                    EntityName = nameof(Alert),
                    EntityId = alert.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Updated alert to IncidentId: {alert.IncidentId}",
                    IPAddress = ip,
                    UserAgent = userAgent
                });

                await _context.AlertHistories.AddAsync(new AlertHistory
                {
                    AlertId = alert.Id,
                    Status = "Updated",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return new AlertResponse { Id = alert.Id, IncidentId = alert.IncidentId, CreatedAt = alert.CreatedAt, Success = true };
        }

        public async Task DeleteAlertAsync(int id, string userId, string ip, string userAgent)
        {
            var actualUserId = await GetValidUserIdAsync(userId);
            var alert = await _context.Alerts.FindAsync(id);
            if (alert == null) throw new KeyNotFoundException("Alert not found.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = actualUserId,
                    Action = "Delete",
                    EntityName = nameof(Alert),
                    EntityId = alert.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Deleted alert for IncidentId: {alert.IncidentId}",
                    IPAddress = ip,
                    UserAgent = userAgent
                });

                _context.Alerts.Remove(alert);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<AlertResponse> GetAlertByIdAsync(int id, string lang = "en")
        {
            var alert = await _context.Alerts
                .Include(a => a.Incident)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alert == null) return null;

            return MapToResponse(alert, lang);
        }

        public async Task<List<AlertResponse>> GetAllAlertsAsync(string lang = "en")
        {
            var alerts = await _context.Alerts.Include(a => a.Incident).ToListAsync();
            return alerts.Select(a => MapToResponse(a, lang)).ToList();
        }

        public async Task<List<AlertResponse>> GetAlertsByRegionAsync(int locationId, string lang = "en")
        {
            var alerts = await _context.Alerts
                .Include(a => a.Incident)
                .Where(a => a.Incident.LocationId == locationId)
                .ToListAsync();

            return alerts.Select(a => MapToResponse(a, lang)).ToList();
        }

        private AlertResponse MapToResponse(Alert alert, string lang)
        {
            bool isAr = lang.ToLower() == "ar";
            var title = isAr ? alert.Incident?.TitleAr : alert.Incident?.Title;
            var prefix = isAr ? "تنبيه منطقة: " : "Regional Alert: ";

            return new AlertResponse
            {
                Id = alert.Id,
                IncidentId = alert.IncidentId,
                CreatedAt = alert.CreatedAt,
                Success = true,
                Message = $"{prefix}{title}"
            };
        }
    }
}
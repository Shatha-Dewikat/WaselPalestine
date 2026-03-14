using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.BLL.Service
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
            return SYSTEM_USER_ID;
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
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return new AlertResponse
            {
                Id = alert.Id,
                IncidentId = alert.IncidentId,
                CreatedAt = alert.CreatedAt
            };
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
                await _context.SaveChangesAsync();

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
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return new AlertResponse
            {
                Id = alert.Id,
                IncidentId = alert.IncidentId,
                CreatedAt = alert.CreatedAt
            };
        }

        public async Task DeleteAlertAsync(int id, string userId, string ip, string userAgent)
        {
            var actualUserId = await GetValidUserIdAsync(userId);
            var alert = await _context.Alerts.FindAsync(id);
            if (alert == null) throw new KeyNotFoundException("Alert not found.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Alerts.Remove(alert);
                await _context.SaveChangesAsync();

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

            var incident = alert.Incident;

            
            var message = lang == "ar"
                ? $"تم الإبلاغ عن حادث: {incident.TitleAr}"
                : $"New incident reported: {incident.Title}";

            return new AlertResponse
            {
                Id = alert.Id,
                IncidentId = alert.IncidentId,
                CreatedAt = alert.CreatedAt,
                Message = message
            };
        }

        public async Task<List<AlertResponse>> GetAllAlertsAsync(string lang = "en")
        {
            var alerts = await _context.Alerts.Include(a => a.Incident).ToListAsync();

            return alerts.Select(a => new AlertResponse
            {
                Id = a.Id,
                IncidentId = a.IncidentId,
                CreatedAt = a.CreatedAt,
                Message = lang == "ar"
                    ? $"تم الإبلاغ عن حادث: {a.Incident?.TitleAr}"
                    : $"New incident reported: {a.Incident?.Title}"
            }).ToList();
        }
    }
}

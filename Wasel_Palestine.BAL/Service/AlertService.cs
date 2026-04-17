using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.BAL.Service;

namespace Wasel_Palestine.BAL.Service
{
    public class AlertService : IAlertService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private const string SYSTEM_USER_ID = "SYSTEM_USER_ID_FROM_DB";

        public AlertService(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        private async Task<string> GetValidUserIdAsync(string userId)
        {
            if (!string.IsNullOrEmpty(userId) && await _context.Users.AnyAsync(u => u.Id == userId))
                return userId;

            return await _context.Users.Select(u => u.Id).FirstOrDefaultAsync();
        }

        public async Task<AlertResponse> CreateAlertAsync(AlertCreateRequest request, string userId, string ip, string userAgent)
        {
            var actualUserId = await GetValidUserIdAsync(userId);

            var incident = await _context.Incidents
                .FirstOrDefaultAsync(i => i.Id == request.IncidentId);

            if (incident == null)
                throw new KeyNotFoundException($"Incident with ID {request.IncidentId} not found.");

            var alert = new Alert
            {
                IncidentId = request.IncidentId,
                CreatedAt = DateTime.UtcNow,
                Title = $"تنبيه: {incident.TitleAr}",
                Message = incident.DescriptionAr ?? $"يوجد تحديث جديد بخصوص: {incident.TitleAr}"
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Alerts.AddAsync(alert);
                await _context.SaveChangesAsync();

                var subscribers = await _context.AlertSubscriptions
                    .Where(s => s.LocationId == incident.LocationId && s.CategoryId == incident.CategoryId)
                    .Select(s => new { s.UserId, s.User.Email })
                    .Distinct()
                    .ToListAsync();

                var recipients = subscribers.Select(sub => new AlertRecipient
                {
                    AlertId = alert.Id,
                    UserId = sub.UserId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                if (recipients.Any())
                {
                    await _context.AlertRecipients.AddRangeAsync(recipients);
                }

                await _context.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = actualUserId,
                    Action = "Create",
                    EntityName = nameof(Alert),
                    EntityId = alert.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Auto-created alert for Incident {incident.Id} and sent to {recipients.Count} subscribers.",
                    IPAddress = ip,
                    UserAgent = userAgent
                });

                await _context.AlertHistories.AddAsync(new AlertHistory
                {
                    AlertId = alert.Id,
                    Status = "Created & Dispatched",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                foreach (var sub in subscribers)
                {
                    if (!string.IsNullOrEmpty(sub.Email))
                    {
                        string readUrl = $"http://localhost:32772/api/v1/Reports/mark-as-read?alertId={alert.Id}&userId={sub.UserId}";
                        string unsubscribeUrl = $"http://localhost:32772/api/v1/Alerts/unsubscribe?alertId={alert.Id}&userId={sub.UserId}";
                        string emailHtml = $@"
                            <div dir='rtl' style='font-family: Arial, sans-serif; border: 1px solid #ddd; padding: 25px; text-align: right; max-width: 600px; margin: auto;'>
                                <h2 style='color: #d9534f; border-bottom: 2px solid #d9534f; padding-bottom: 10px;'>تنبيه طارئ: {alert.Title}</h2>
                                <p style='font-size: 16px; color: #333;'>نود إعلامك بوقوع حادث جديد في منطقة تتابعها:</p>
                                <blockquote style='background: #f9f9f9; padding: 15px; border-right: 5px solid #d9534f; margin: 20px 0; color: #555;'>
                                    {alert.Message}
                                </blockquote>
                                <div style='text-align: center; margin: 30px 0;'>
                                    <a href='{readUrl}' style='background-color: #28a745; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px;'>لقد قرأت هذا التنبيه</a>
                                </div>
                                <hr style='border: 0; border-top: 1px solid #eee; margin-top: 30px;'>
                                <p style='font-size: 12px; color: #888;'>
                                    وصلك هذا الإيميل لأنك مشترك في نظام واصل فلسطين. 
                                    <br/>
                                    لا تريد استقبال هذه التنبيهات؟ <a href='{unsubscribeUrl}' style='color: #d9534f; text-decoration: underline;'>إلغاء الاشتراك من هذه المنطقة</a>
                                </p>
                            </div>";

                        await _emailSender.SendEmailAsync(sub.Email, alert.Title, emailHtml);
                    }
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Failed to create alert and notify subscribers.", ex);
            }

            return new AlertResponse
            {
                Id = alert.Id,
                IncidentId = alert.IncidentId,
                CreatedAt = alert.CreatedAt,
                Success = true
            };
        }

        public async Task<object> GetAlertsStatisticsAsync()
        {
            var totalAlerts = await _context.Alerts.CountAsync();
            var totalSentRecipients = await _context.AlertRecipients.CountAsync();
            var readCount = await _context.AlertRecipients.CountAsync(r => r.IsRead);

            double engagementRate = totalSentRecipients > 0
                ? Math.Round((double)readCount / totalSentRecipients * 100, 2)
                : 0;

            return new
            {
                TotalAlertsCreated = totalAlerts,
                TotalNotificationsSent = totalSentRecipients,
                TotalOpened = readCount,
                EngagementRate = engagementRate + "%"
            };
        }

        public async Task<bool> UnsubscribeFromAlertsAsync(int alertId, string userId)
        {
            var alert = await _context.Alerts
                .Include(a => a.Incident)
                .FirstOrDefaultAsync(a => a.Id == alertId);

            if (alert == null) return false;

            var subscription = await _context.AlertSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId &&
                                         s.LocationId == alert.Incident.LocationId &&
                                         s.CategoryId == alert.Incident.CategoryId);

            if (subscription != null)
            {
                _context.AlertSubscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
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

            _context.Alerts.Remove(alert);
            await _context.SaveChangesAsync();
        }

        public async Task<AlertResponse> GetAlertByIdAsync(int id, string lang = "en")
        {
            var alert = await _context.Alerts.Include(a => a.Incident).FirstOrDefaultAsync(a => a.Id == id);
            return alert == null ? null : MapToResponse(alert, lang);
        }

        public async Task<List<AlertResponse>> GetAllAlertsAsync(string lang = "en")
        {
            var alerts = await _context.Alerts.Include(a => a.Incident).ToListAsync();
            return alerts.Select(a => MapToResponse(a, lang)).ToList();
        }

        public async Task<List<AlertResponse>> GetAlertsByRegionAsync(int locationId, string lang = "en")
        {
            var alerts = await _context.Alerts.Include(a => a.Incident).Where(a => a.Incident.LocationId == locationId).ToListAsync();
            return alerts.Select(a => MapToResponse(a, lang)).ToList();
        }

        public async Task<string> SubscribeToAlertAsync(SubscribeAlertDto subscriptionDto)
        {
            var isSubscribed = await _context.AlertSubscriptions.AnyAsync(s => s.UserId == subscriptionDto.UserId && s.LocationId == subscriptionDto.LocationId && s.CategoryId == subscriptionDto.CategoryId);
            if (isSubscribed) return "You are already subscribed.";

            _context.AlertSubscriptions.Add(new AlertSubscription { UserId = subscriptionDto.UserId, LocationId = subscriptionDto.LocationId, CategoryId = subscriptionDto.CategoryId, CreatedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            return "Success: You will now receive alerts for this area.";
        }

        private AlertResponse MapToResponse(Alert alert, string lang)
        {
            bool isAr = lang.ToLower() == "ar";
            return new AlertResponse { Id = alert.Id, IncidentId = alert.IncidentId, CreatedAt = alert.CreatedAt, Success = true, Message = $"{(isAr ? "تنبيه منطقة: " : "Regional Alert: ")}{(isAr ? alert.Incident?.TitleAr : alert.Incident?.Title)}" };
        }
    }
}
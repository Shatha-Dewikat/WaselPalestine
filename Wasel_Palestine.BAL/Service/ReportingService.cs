using Microsoft.EntityFrameworkCore;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.BAL.DTOs;

namespace Wasel_Palestine.BAL.Service
{
    public class ReportingService
    {
        private readonly ApplicationDbContext _context;

        public ReportingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> SubmitReportAsync(CreateReportDto reportDto, bool isStaff)
        {
            var thresholdTime = DateTime.UtcNow.AddHours(-2);

            float initialScore = isStaff ? 1.0f : 0.4f;

            var potentialDuplicates = await _context.Reports
                .Include(r => r.Location)
                .Where(r => r.CategoryId == reportDto.CategoryId && r.CreatedAt >= thresholdTime)
                .ToListAsync();

            var existingDuplicate = potentialDuplicates
                .FirstOrDefault(r => CalculateDistance((double)reportDto.Latitude, (double)reportDto.Longitude,
                                                       (double)r.Location.Latitude, (double)r.Location.Longitude) <= 0.5);

            var newLocation = new Location
            {
                Latitude = reportDto.Latitude,
                Longitude = reportDto.Longitude,
                AreaName = "Manual Report",
                City = "Unknown",
                CreatedAt = DateTime.UtcNow
            };

            var newReport = new Report
            {
                Location = newLocation,
                Description = reportDto.Description ?? "No description",
                CategoryId = reportDto.CategoryId,
                UserId = reportDto.UserId,
                CreatedAt = DateTime.UtcNow,
                StatusId = isStaff ? 2 : 1,
                ConfidenceScore = initialScore
            };

            if (existingDuplicate != null)
            {
                newReport.DuplicateOfReportId = existingDuplicate.Id;

                if (isStaff)
                {
                    existingDuplicate.ConfidenceScore = 1.0f;
                    existingDuplicate.StatusId = 2;
                }
                else if (existingDuplicate.UserId != reportDto.UserId)
                {
                    existingDuplicate.ConfidenceScore += 0.3f;
                }

                if (existingDuplicate.ConfidenceScore >= 1.0f)
                {
                    bool alreadyHasIncident = await _context.Incidents
                        .AnyAsync(i => i.LocationId == existingDuplicate.LocationId
                                  && i.CategoryId == existingDuplicate.CategoryId
                                  && i.CreatedAt >= thresholdTime);

                    if (!alreadyHasIncident)
                    {
                        await CreateIncidentFromReportAsync(existingDuplicate);
                    }
                }
            }
            else if (isStaff)
            {
                _context.Reports.Add(newReport);
                await _context.SaveChangesAsync();
                await CreateIncidentFromReportAsync(newReport);
                return "Report confirmed and published immediately by staff.";
            }

            _context.Reports.Add(newReport);
            await _context.SaveChangesAsync();

         
            if (isStaff) return "Report confirmed and published immediately by staff.";

            if (existingDuplicate != null)
            {
                if (existingDuplicate.UserId == reportDto.UserId)
                    return "You have already reported this incident. Thank you.";

                if (existingDuplicate.ConfidenceScore >= 1.0f)
                    return "Thank you! The incident has been confirmed and published.";

                return "Thank you! Your confirmation has been added to the report.";
            }

            return "Thank you! Your report has been received and is under review.";
        }

        private async Task CreateIncidentFromReportAsync(Report report)
        {
            var confirmedStatus = await _context.IncidentStatuses.FirstOrDefaultAsync(s => s.Name == "Confirmed");
            var mediumSeverity = await _context.IncidentSeverities.FirstOrDefaultAsync(s => s.Name == "Medium");

            // 1. إنشاء الحادث أولاً
            var newIncident = new Incident
            {
                Title = "Reported Incident",
                Description = report.Description,
                TitleAr = "بلاغ عن حادث/إغلاق",
                DescriptionAr = report.Description ?? "لا يوجد وصف إضافي",
                LocationId = report.LocationId,
                CategoryId = report.CategoryId,
                StatusId = confirmedStatus?.Id ?? 2,
                SeverityId = mediumSeverity?.Id ?? 1,
                CreatedAt = DateTime.UtcNow,
                Verified = true,
                VerifiedAt = DateTime.UtcNow,
                CreatedByUserId = report.UserId,
                IncidentMedia = new List<IncidentMedia>()
            };

            _context.Incidents.Add(newIncident);

            // ** خطوة حاسمة: حفظ الحادث أولاً ليأخذ Id من قاعدة البيانات **
            await _context.SaveChangesAsync();

            // 2. الآن ننشئ التنبيه ونربطه بالـ Id الجديد
            var alert = new Alert
            {
                Title = "تنبيه جديد: " + newIncident.TitleAr,
                Message = newIncident.DescriptionAr,
                CreatedAt = DateTime.UtcNow,
                // تأكد أن اسم الحقل في جدول Alerts هو IncidentId
                IncidentId = newIncident.Id
            };

            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync(); // حفظ التنبيه

            // 3. جلب المشتركين وإرسال الإشعارات لهم
            var subscribers = await _context.AlertSubscriptions
                .Where(s => s.CategoryId == report.CategoryId)
                .ToListAsync();

            foreach (var sub in subscribers)
            {
                _context.AlertRecipients.Add(new AlertRecipient
                {
                    AlertId = alert.Id,
                    UserId = sub.UserId
                });
            }

            await _context.SaveChangesAsync();
        }



        public async Task<string> DismissReportAsync(int reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return "Error: Report not found.";

            report.ConfidenceScore -= 0.2f;
            if (report.ConfidenceScore < 1.0f && report.StatusId == 2) report.StatusId = 1;
            if (report.ConfidenceScore <= 0.31f) report.StatusId = 4; // Dismissed

            await _context.SaveChangesAsync();
            return "Thank you! Your feedback has been recorded.";
        }

        public async Task<List<ActiveReportDto>> GetActiveReportsAsync()
        {
            return await _context.Reports
                .Include(r => r.Category)
                .Include(r => r.Location)
                .Where(r => r.StatusId == 2)
                .Select(r => new ActiveReportDto
                {
                    Id = r.Id,
                    Description = r.Description,
                    Latitude = (double)r.Location.Latitude,
                    Longitude = (double)r.Location.Longitude,
                    CategoryName = r.Category.Name,
                    ConfidenceScore = r.ConfidenceScore,
                    CreatedAt = r.CreatedAt
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<string> SubscribeToAlertAsync(SubscribeAlertDto subscriptionDto)
        {
            var isSubscribed = await _context.AlertSubscriptions
                .AnyAsync(s => s.UserId == subscriptionDto.UserId &&
                               s.LocationId == subscriptionDto.LocationId &&
                               s.CategoryId == subscriptionDto.CategoryId);

            if (isSubscribed) return "You are already subscribed to alerts.";

            var subscription = new AlertSubscription
            {
                UserId = subscriptionDto.UserId,
                LocationId = subscriptionDto.LocationId,
                CategoryId = subscriptionDto.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            _context.AlertSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            return "Success: You will now receive alerts for this location.";
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371;
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double angle) => (Math.PI / 180) * angle;
    }
}
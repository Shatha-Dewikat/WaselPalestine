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

        public async Task<string> SubmitReportAsync(CreateReportDto reportDto)
        {
            if (reportDto.Latitude < 31.0m || reportDto.Latitude > 33.5m || reportDto.Longitude < 34.0m || reportDto.Longitude > 35.5m)
            {
                return "Error: Location is outside the supported geographical area.";
            }

            var thresholdTime = DateTime.Now.AddHours(-2);

            var existingReports = await _context.Reports
                .Include(r => r.Location)
                .Where(r => r.CategoryId == reportDto.CategoryId && r.CreatedAt >= thresholdTime)
                .ToListAsync();

            int? duplicateOfId = null;

            foreach (var report in existingReports)
            {
                double distance = CalculateDistance(
                    (double)reportDto.Latitude, (double)reportDto.Longitude,
                    (double)report.Location.Latitude, (double)report.Location.Longitude);

                if (distance <= 0.5) 
                {
                    duplicateOfId = report.Id;
                    
                    report.ConfidenceScore += 0.2f;

                    if (report.ConfidenceScore >= 1.0f && report.StatusId != 2)
                    {
                        report.StatusId = 2; 
                        await TriggerAlertsForSubscribers(report);
                    }
                    break;
                }
            }

            var newReport = new Report
            {
                Location = new Location
                {
                    Latitude = reportDto.Latitude,
                    Longitude = reportDto.Longitude,
                    AreaName = "Manual Report",
                    City = "Unknown",
                    CreatedAt = DateTime.Now
                },
                Description = reportDto.Description,
                CategoryId = reportDto.CategoryId,
                UserId = reportDto.UserId,
                CreatedAt = DateTime.Now,
                StatusId = 1,
                ConfidenceScore = 0.5f,
                DuplicateOfReportId = duplicateOfId
            };

            _context.Reports.Add(newReport);
            await _context.SaveChangesAsync();

            return duplicateOfId != null 
                ? "Thank you! Your report confirmed an existing incident." 
                : "Success: New report submitted successfully.";
        }

        private async Task TriggerAlertsForSubscribers(Report report)
        {
            var subscribers = await _context.AlertSubscriptions
                .Where(s => s.LocationId == report.LocationId && s.CategoryId == report.CategoryId)
                .ToListAsync();

            foreach (var sub in subscribers)
            {
                var alert = new AlertHistory 
                {
                    AlertId = report.Id,
                    Status = "Confirmed", 
                    Timestamp = DateTime.Now 
                };
                _context.AlertHistories.Add(alert);
            }
        }

        public async Task<string> DismissReportAsync(int reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);

            if (report == null) return "Error: Report not found.";

            report.ConfidenceScore -= 0.2f;

            if (report.ConfidenceScore < 1.0f && report.StatusId == 2)
            {
                report.StatusId = 1; 
            }

            if (report.ConfidenceScore <= 0.31f)
            {
                report.StatusId = 4;
            }

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

            if (isSubscribed)
                return "You are already subscribed to alerts for this location and category.";

            var subscription = new AlertSubscription
            {
                UserId = subscriptionDto.UserId,
                LocationId = subscriptionDto.LocationId,
                CategoryId = subscriptionDto.CategoryId,
                CreatedAt = DateTime.Now
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
// Haversine Formula
// a = sin^2(Δφ / 2) + cos(φ1) * cos(φ2) * sin^2(Δλ / 2)
// c = 2 * atan2( sqrt(a), sqrt(1 − a) )
// distance = R * c
//
// where:
// φ1, φ2 = latitude of point 1 and point 2 (in radians)
// λ1, λ2 = longitude of point 1 and point 2 (in radians)
// Δφ = φ2 − φ1
// Δλ = λ2 − λ1
// R = Earth's radius (≈ 6371 km)


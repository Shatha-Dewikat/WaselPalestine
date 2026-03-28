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
            
            var thresholdTime = DateTime.Now.AddHours(-2);

            
            var existingReports = await _context.Reports
                .Include(r => r.Location)
                .Where(r => r.CategoryId == reportDto.CategoryId && r.CreatedAt >= thresholdTime)
                .ToListAsync();

            Report existingDuplicate = null;

            
            foreach (var report in existingReports)
            {
                double distance = CalculateDistance(
                    (double)reportDto.Latitude, (double)reportDto.Longitude,
                    (double)report.Location.Latitude, (double)report.Location.Longitude);

                if (distance <= 0.5)
                {
                    existingDuplicate = report;
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
                ConfidenceScore = 0.5f
            };

           
            if (existingDuplicate != null)
            {
             
                newReport.DuplicateOfReportId = existingDuplicate.Id;

                
                existingDuplicate.ConfidenceScore += 0.2f;

                
                if (existingDuplicate.ConfidenceScore >= 1.0f)
                {
                    existingDuplicate.StatusId = 2;
                }

                _context.Reports.Add(newReport);
                await _context.SaveChangesAsync();

                return "Thank you! Your report confirmed an existing incident and improved its reliability.";
            }

           
            _context.Reports.Add(newReport);
            await _context.SaveChangesAsync();

            return "Success: New report submitted successfully.";
        }
       
       public async Task<string> DismissReportAsync(int reportId)
{
    var report = await _context.Reports.FindAsync(reportId);

    if (report == null)
        return "Error: Report not found.";

   
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


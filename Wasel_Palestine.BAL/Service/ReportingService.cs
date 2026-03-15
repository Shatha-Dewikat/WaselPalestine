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
            // 1. تحديد النطاق الزمني (مثلاً آخر ساعتين فقط)
            var thresholdTime = DateTime.Now.AddHours(-2);

            var existingReports = await _context.Reports
                .Include(r => r.Location)
                .Where(r => r.CategoryId == reportDto.CategoryId && r.CreatedAt >= thresholdTime)
                .ToListAsync();

          
            foreach (var report in existingReports)
            {
                double distance = CalculateDistance(
                    (double)reportDto.Latitude, (double)reportDto.Longitude,
                    (double)report.Location.Latitude, (double)report.Location.Longitude);

                if (distance <= 0.5) // 0.5 كم تعني 500 متر
                {
                    return "Duplicate: A similar report already exists in this area.";
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
                UserId = reportDto.UserId ?? "Anonymous",
                CreatedAt = DateTime.Now,
                StatusId = 1 
            };

            _context.Reports.Add(newReport);
            await _context.SaveChangesAsync();
            
            return "Success: Report submitted successfully.";
        }

        //(Haversine Formula)
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
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
        public async Task<bool> SubmitReportAsync(CreateReportDto reportDto)
        {
         var newReport = new Report
            {
             Location = new Location { Latitude = reportDto.Latitude, 
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
            return await _context.SaveChangesAsync() > 0;


        }

    }

}


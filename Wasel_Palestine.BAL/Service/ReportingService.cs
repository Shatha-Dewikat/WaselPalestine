using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Geometries;
using Wasel_Palestine.BAL;
using Wasel_Palestine.BAL.Service;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.BAL.Service
{
    public class ReportingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IAlertService _alertService;
        private readonly IMemoryCache _cache;

        public ReportingService(ApplicationDbContext context, IWebHostEnvironment environment, IAlertService alertService, IMemoryCache cache)
        {
            _context = context;
            _environment = environment;
            _alertService = alertService;
            _cache = cache;
        }

        public async Task<string> SubmitReportAsync(CreateReportDto reportDto, bool isStaff)

        {

            var thresholdTime = DateTime.UtcNow.AddHours(-2);

            float staffScore = 1.0f;

            float userInitialScore = 0.4f;

            float confirmationIncrement = 0.2f;



            var potentialDuplicates = await _context.Reports

                .Include(r => r.Location)

                .Where(r => r.DeletedAt == null &&

                            r.CategoryId == reportDto.CategoryId &&

                            r.CreatedAt >= thresholdTime)

                .ToListAsync();



            var existingDuplicate = potentialDuplicates

                .FirstOrDefault(r => CalculateDistance((double)reportDto.Latitude, (double)reportDto.Longitude,

                                                       (double)r.Location.Latitude, (double)r.Location.Longitude) <= 0.5);



            if (existingDuplicate != null)

            {

                if (existingDuplicate.UserId == reportDto.UserId)

                    return "You have already reported this incident.";



                existingDuplicate.ConfidenceScore = isStaff ? staffScore : Math.Min(1.0f, existingDuplicate.ConfidenceScore + confirmationIncrement);

                existingDuplicate.CreatedAt = DateTime.UtcNow;



                if (existingDuplicate.ConfidenceScore >= 1.0f)

                {

                    var alreadyHasIncident = await _context.Incidents.AnyAsync(i => i.LocationId == existingDuplicate.LocationId && i.CreatedAt >= thresholdTime);

                    if (!alreadyHasIncident) await CreateIncidentFromReportAsync(existingDuplicate);

                }



                await _context.SaveChangesAsync();

                return "Thank you! Your confirmation has been added.";

            }



            var existingLocation = await _context.Locations

                .FirstOrDefaultAsync(l => Math.Abs(l.Latitude - reportDto.Latitude) < 0.0001m &&

                                         Math.Abs(l.Longitude - reportDto.Longitude) < 0.0001m);



            DAL.Model.Location locationToUse;

            if (existingLocation != null)

            {

                locationToUse = existingLocation;

            }

            else

            {

                locationToUse = new DAL.Model.Location

                {

                    Latitude = reportDto.Latitude,

                    Longitude = reportDto.Longitude,

                    City = reportDto.City,

                    AreaName = reportDto.AreaName,

                    CreatedAt = DateTime.UtcNow,

                    Coordinates = new Point((double)reportDto.Longitude, (double)reportDto.Latitude) { SRID = 4326 }

                };

            }



            var allCheckpoints = await _context.Checkpoints.Include(c => c.Location).ToListAsync();

            var nearbyCheckpoint = allCheckpoints

                .FirstOrDefault(c => CalculateDistance((double)reportDto.Latitude, (double)reportDto.Longitude,

                                                       (double)c.Location.Latitude, (double)c.Location.Longitude) <= 1.0);



            var newReport = new Report

            {

                Location = locationToUse,

                Description = reportDto.Description ?? "بلاغ عن " + locationToUse.City,

                CategoryId = reportDto.CategoryId,

                UserId = reportDto.UserId,

                CreatedAt = DateTime.UtcNow,

                StatusId = isStaff ? 2 : 1,

                ConfidenceScore = isStaff ? staffScore : userInitialScore,

                DeletedAt = null,

                CheckpointId = nearbyCheckpoint?.Id

            };



            _context.Reports.Add(newReport);

            await _context.SaveChangesAsync();



            if (isStaff) await CreateIncidentFromReportAsync(newReport);



            return "Thank you! Your report has been received." + (nearbyCheckpoint != null ? $" Linked to {nearbyCheckpoint.NameAr}." : "");

        }



        private async Task CreateIncidentFromReportAsync(Report report)

        {

            var confirmedStatus = await _context.IncidentStatuses.FirstOrDefaultAsync(s => s.Name == "Confirmed");

            var mediumSeverity = await _context.IncidentSeverities.FirstOrDefaultAsync(s => s.Name == "Medium");



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

            await _context.SaveChangesAsync();



            var alertRequest = new AlertCreateRequest

            {

                IncidentId = newIncident.Id

            };



            await _alertService.CreateAlertAsync(alertRequest, "SYSTEM", "127.0.0.1", "System-Auto");

        }



        public async Task<string> UploadReportMediaAsync(int reportId, string userId, IFormFile file)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return "Report not found";

            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/reports");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var media = new ReportMedia
            {
                ReportId = reportId,
                UserId = userId,
                MediaUrl = "/uploads/reports/" + uniqueFileName,
                MediaType = file.ContentType.StartsWith("image") ? "Image" : "Video",
                CreatedAt = DateTime.UtcNow
            };

            report.ConfidenceScore = Math.Min(1.0f, report.ConfidenceScore + 0.3f);
            _context.ReportMedias.Add(media);
            await _context.SaveChangesAsync();

            return "Media uploaded and confidence score increased!";
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

        public async Task<string> VoteOnReportAsync(int reportId, string userId, bool isUpvote)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null || report.DeletedAt != null) return "Report not found";

            var existingVote = await _context.ReportVotes
                .FirstOrDefaultAsync(v => v.ReportId == reportId && v.UserId == userId);

            if (existingVote != null) return "You have already voted on this report.";

            var vote = new ReportVote
            {
                ReportId = reportId,
                UserId = userId,
                VoteType = isUpvote ? "Upvote" : "Downvote",
                CreatedAt = DateTime.UtcNow
            };

            float change = isUpvote ? 0.1f : -0.1f;
            report.ConfidenceScore = Math.Clamp(report.ConfidenceScore + change, 0, 1.0f);

            _context.ReportVotes.Add(vote);
            await _context.SaveChangesAsync();

            return isUpvote ? "Report confirmed!" : "Report disputed.";
        }

        public async Task<string> DeleteReportAsync(int reportId, string adminId, string reason)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return "Report not found";

            report.DeletedAt = DateTime.UtcNow;

            var action = new ReportModerationAction
            {
                ReportId = reportId,
                ModeratorId = adminId,
                Action = "Soft Delete",
                Notes = reason,
                ActionAt = DateTime.UtcNow
            };

            _context.ReportModerationActions.Add(action);
            await _context.SaveChangesAsync();

            return "Report deleted and action logged.";
        }

        public async Task<List<HeatmapPointResponse>> GetIncidentHeatmapAsync(DateTime? fromDate)
        {
            var query = _context.Reports.AsQueryable();
            query = query.Where(r => r.DeletedAt == null && r.StatusId != 3);

            if (fromDate.HasValue)
                query = query.Where(r => r.CreatedAt >= fromDate.Value);

            return await query
                .GroupBy(r => new { r.Location.Latitude, r.Location.Longitude })
                .Select(g => new HeatmapPointResponse
                {
                    Latitude = (double)g.Key.Latitude,
                    Longitude = (double)g.Key.Longitude,
                    Intensity = g.Count()
                }).ToListAsync();
        }

        public async Task<string> UpdateReportStatusAsync(int reportId, int newStatusId, string moderatorId, string notes)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null || report.DeletedAt != null) return "Report not found";

            report.StatusId = newStatusId;
            if (newStatusId == 3) report.ConfidenceScore = 0;

            var action = new ReportModerationAction
            {
                ReportId = reportId,
                ModeratorId = moderatorId,
                Action = $"Status changed to {newStatusId}",
                Notes = notes,
                ActionAt = DateTime.UtcNow
            };

            _context.ReportModerationActions.Add(action);
            await _context.SaveChangesAsync();

            return "Status updated and impact applied.";
        }

        public async Task<string> DismissReportAsync(int reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return "Error: Report not found.";

            report.ConfidenceScore -= 0.2f;
            if (report.ConfidenceScore < 1.0f && report.StatusId == 2) report.StatusId = 1;
            if (report.ConfidenceScore <= 0.31f) report.StatusId = 4;

            await _context.SaveChangesAsync();
            return "Thank you! Your feedback has been recorded.";
        }

        public async Task<List<ActiveReportDto>> GetActiveReportsAsync()
        {
            return await _context.Reports
                .Include(r => r.Category)
                .Include(r => r.Location)
                .Where(r => r.DeletedAt == null && r.StatusId == 2)
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

        public async Task<bool> MarkAlertAsReadAsync(int alertId, string userId)
        {
            var recipient = await _context.AlertRecipients
                .FirstOrDefaultAsync(r => r.AlertId == alertId && r.UserId == userId);

            if (recipient == null) return false;

            recipient.IsRead = true;
            _context.AlertRecipients.Update(recipient); 

            return await _context.SaveChangesAsync() > 0; 
        }
    }


}
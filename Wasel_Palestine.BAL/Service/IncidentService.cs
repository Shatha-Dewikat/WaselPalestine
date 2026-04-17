using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wasel_Palestine.BAL.Helper;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Migrations;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Repository;

namespace Wasel_Palestine.BAL.Service
{
    public class IncidentService : IIncidentService
    {
        private readonly IIncidentRepository _incidentRepo;
        private readonly IAlertService _alertService;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWeatherService _weatherService;
        private readonly IMemoryCache _cache;
        private const string DashboardCacheKey = "CityStatsCache";
        public IncidentService(IIncidentRepository incidentRepo, IAlertService alertService, IWeatherService weatherService, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IMemoryCache cache)
        {
            _incidentRepo = incidentRepo;
            _alertService = alertService;
            _weatherService = weatherService;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
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
                await _context.Entry(incident).Reference(i => i.Category).LoadAsync();
                await _context.Entry(incident).Reference(i => i.Severity).LoadAsync();
                await _context.Entry(incident).Reference(i => i.Status).LoadAsync();

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

            _context.IncidentHistories.Add(new IncidentHistory
            {
                IncidentId = id,
                StatusId = incident.StatusId,
                ChangedByUserId = userId,
                ChangedAt = DateTime.UtcNow,
                Action = "Updated",
                Changes = "Updated incident details" 
            });
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

            _context.IncidentHistories.Add(new IncidentHistory
            {
                IncidentId = incidentId,
                StatusId = status.Id,
                ChangedByUserId = userId,
                ChangedAt = DateTime.UtcNow,
                Changes = "Incident status changed to Verified",
                Action = "Verified"
            });

            await _alertService.CreateAlertAsync(new AlertCreateRequest
            {
                IncidentId = incidentId
            }, userId, GetIP(), GetUserAgent());

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
            _context.IncidentHistories.Add(new IncidentHistory
            {
                IncidentId = id,
                StatusId = status.Id,
                ChangedByUserId = userId,
                ChangedAt = DateTime.UtcNow,
                Action = statusName,
                Changes = $"Status changed to {statusName}" 
            }); await _context.SaveChangesAsync();
            return new SimpleResponse { Success = true };
        }
        #endregion

        public async Task<List<CityIncidentStats>> GetDashboardStatsAsync()
        {
           
            if (_cache == null)
                throw new Exception("MemoryCache is not initialized in Constructor.");

            if (!_cache.TryGetValue(DashboardCacheKey, out List<CityIncidentStats> stats))
            {
               
                var incidents = await _context.Incidents
                    .Include(i => i.Location)
                    .Include(i => i.Status)
                    
                    .Where(i => i.Status != null && i.Status.Name != "Resolved" && i.Status.Name != "Closed")
                    .ToListAsync();

                
                var groupedStats = incidents
                    .GroupBy(i => i.Location?.City ?? "Unknown")
                    .Select(group => new CityIncidentStats
                    {
                        City = group.Key,
                        ActiveIncidentsCount = group.Count(),
                        
                        ClosedCheckpointsCount = _context.Checkpoints
                            .Include(c => c.Location)
                            .AsEnumerable()
                            .Count(c => (c.Location?.City ?? "Unknown") == group.Key && c.CurrentStatus == "Closed")
                    })
                    .ToList();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                _cache.Set(DashboardCacheKey, groupedStats, cacheOptions);
                return groupedStats;
            }

            return stats;
        }

        #region Queries
        public async Task<IncidentResponse> GetIncidentByIdAsync(int id, string lang = "en")
        {
            var incident = await _context.Incidents
                .Include(i => i.Category)
                .Include(i => i.Status)
                .Include(i => i.Severity)
                .Include(i => i.Location)
                .Include(i => i.IncidentMedia) 
                .Include(i => i.IncidentHistories)
                .FirstOrDefaultAsync(i => i.Id == id);

            return incident == null ? new IncidentResponse { Success = false } : MapToResponse(incident, lang);
        }
        public async Task<List<IncidentResponse>> GetIncidentAllAsync(string lang = "en")
        {
            var incidents = await _context.Incidents
                .Include(i => i.Category)
                .Include(i => i.Severity)
                .Include(i => i.Status)
                .Include(i => i.Location)
                .Include(i => i.IncidentMedia)
                .ToListAsync();

            return incidents.Select(i => MapToResponse(i, lang)).ToList();
        }

        public async Task<List<IncidentHistoryResponse>> GetIncidentHistoryAsync(int incidentId)
        {
            var histories = await _context.IncidentHistories.Where(h => h.IncidentId == incidentId).ToListAsync();
            return histories.Select(h => new IncidentHistoryResponse { IncidentId = h.IncidentId, StatusId = h.StatusId, Action = h.Action, ChangedAt = h.ChangedAt }).ToList();
        }

        public async Task<List<IncidentResponse>> GetFilteredIncidentsAsync(IncidentFilterRequest filter, string lang = "en")
        {
            var query = _context.Incidents
                 .Include(i => i.Location)
                 .Include(i => i.Category)
                 .Include(i => i.Status)
                 .Include(i => i.Severity) 
                 .Include(i => i.IncidentMedia)
                 .AsQueryable();

            if (filter.StatusId.HasValue) query = query.Where(i => i.StatusId == filter.StatusId);

            var data = await query.ToListAsync();
            return data.Select(i => MapToResponse(i, lang)).ToList();
        }

        public async Task<List<IncidentResponse>> GetPagedIncidentsAsync(PaginationRequest pagination, string lang = "en")
        {
            var data = await _context.Incidents
                .Include(i => i.Location)
                .Include(i => i.Category)
                .Include(i => i.Status)
                .Include(i => i.Severity) // تأكد من وجود هذا السطر
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Include(i => i.IncidentMedia)
                .ToListAsync();
            return data.Select(i => MapToResponse(i, lang)).ToList();
        }

        public async Task<List<IncidentResponse>> GetFilteredPagedIncidentsAsync(IncidentQueryRequest request, string lang = "en")
        {
            var query = _context.Incidents
                .Include(i => i.Location)
                .Include(i => i.Category)
                .Include(i => i.Severity)
                .Include(i => i.Status)
                .Include(i => i.Severity)
                .Include(i => i.IncidentMedia)
                .AsQueryable();

        
            if (request.Filter.StatusId.HasValue)
                query = query.Where(i => i.StatusId == request.Filter.StatusId);

            if (request.Filter.CategoryId.HasValue)
                query = query.Where(i => i.CategoryId == request.Filter.CategoryId);

          
           
            query = query.OrderByDescending(i => i.CreatedAt);

           
            var data = await query
                .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .ToListAsync();

            return data.Select(i => MapToResponse(i, lang)).ToList();
        }
        #endregion

        public async Task AutoCreateWeatherIncidentAsync(double lat, double lon)
        {
            Console.WriteLine($"=== AutoCreateWeatherIncidentAsync ===");
            Console.WriteLine($"Checking location: Lat={lat}, Lon={lon}");

            
            var weather = await _weatherService.GetCurrentWeatherAsync(lat, lon);
            Console.WriteLine($"Weather condition: {weather.Condition}");

           
            var rule = WeatherRuleHelper.MapToIncident(weather.Condition);
            if (rule == null)
            {
                Console.WriteLine("No matching rule found in WeatherRuleHelper for this condition.");
                return;
            }
            Console.WriteLine($"Rule found: Title={rule.Value.Title}, SeverityId={rule.Value.SeverityId}");

       
            var exists = await _context.Incidents.AnyAsync(i =>
                Math.Abs((double)i.Location.Latitude - lat) < 0.1 &&
                Math.Abs((double)i.Location.Longitude - lon) < 0.1 &&
                i.Description == rule.Value.Description &&
                i.CreatedAt > DateTime.UtcNow.AddMinutes(-30)
            );
            Console.WriteLine($"Similar incident exists in last 30 minutes: {exists}");
            if (exists) return;

            
            var category = await _context.IncidentCategories.FirstOrDefaultAsync(c => c.Name == "Weather");
            if (category == null)
            {
                Console.WriteLine("Weather category not found in DB.");
                return;
            }

            string severityName = rule.Value.SeverityId switch
            {
                1 => "Low",
                2 => "Medium",
                3 => "High",
                _ => "Low"
            };

            var severity = await _context.IncidentSeverities.FirstOrDefaultAsync(s => s.Name == severityName);
            if (severity == null)
            {
                severity = await _context.IncidentSeverities.FirstAsync(s => s.Name == "Low");
                Console.WriteLine("Severity not found, defaulted to Low.");
            }


            var openStatus = await _context.IncidentStatuses.FirstOrDefaultAsync(s => s.Name == "Open");
            if (openStatus == null)
            {
                Console.WriteLine("Open status not found in DB.");
                return;
            }

            var incident = new Incident
            {
                Title = rule?.Title ?? "Weather Event",
                TitleAr = rule?.Title ?? "حادث طقس",
                Description = rule?.Description ?? "Weather incident generated automatically",
                DescriptionAr = rule?.Description ?? "حادث الطقس تم إنشاؤه تلقائيًا", 
                CategoryId = category.Id,
                SeverityId = severity.Id,
                StatusId = openStatus.Id,
                CreatedAt = DateTime.UtcNow,
                Location = new Location
                {
                    Latitude = (decimal)lat,
                    Longitude = (decimal)lon,
                    City = "Auto-Generated",
                    AreaName = "Auto-Generated",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.Incidents.Add(incident);
            await _context.SaveChangesAsync();
            await _alertService.CreateAlertAsync(new AlertCreateRequest
            {
                IncidentId = incident.Id
            },
      "SYSTEM_WEATHER_SERVICE", 
      "INTERNAL_SERVER",       
      "Wasel_Weather_Engine_v1" 
      );
        }

        public async Task ProcessWeatherIncidentsAsync()
        {
            
            var expiryTime = DateTime.UtcNow.AddHours(-2);
            var oldIncidents = _context.Incidents
                .Where(i => i.Category.Name == "Weather" && i.CreatedAt < expiryTime);

            _context.Incidents.RemoveRange(oldIncidents);
            await _context.SaveChangesAsync();

           
            var pointsToTrack = new List<(double Lat, double Lon)>();

            var cpLocations = await _context.Checkpoints
                .Include(c => c.Location)
                .Where(c => c.Location != null)
                .Select(c => new { Lat = (double)c.Location.Latitude, Lon = (double)c.Location.Longitude })
                .ToListAsync();

            pointsToTrack.AddRange(cpLocations.Select(x => (x.Lat, x.Lon)));
            pointsToTrack.AddRange(PalestineGridHelper.GenerateGrid(0.5));

            var uniquePoints = pointsToTrack.GroupBy(p => new {
                Lat = Math.Round(p.Lat, 1),
                Lon = Math.Round(p.Lon, 1)
            }).Select(g => g.First()).ToList();

           
            foreach (var loc in uniquePoints)
            {
                await AutoCreateWeatherIncidentAsync(loc.Lat, loc.Lon);
            }
        }
        private IncidentResponse MapToResponse(Incident incident, string lang = "en")
        {
            var baseUrl = "http://localhost:5034";

            return new IncidentResponse
            {
                Id = incident.Id,
                Title = lang == "ar" ? incident.TitleAr : incident.Title,
                Description = lang == "ar" ? incident.DescriptionAr : incident.Description,
                Category = incident.Category?.Name,
                Severity = incident.Severity?.Name,
                Status = incident.Status?.Name,
                Latitude = (double)(incident.Location?.Latitude ?? 0),
                Longitude = (double)(incident.Location?.Longitude ?? 0),
                
     
                CreatedAt = incident.CreatedAt,
                RelatedCheckpointId = incident.CheckpointId,
                Verified = incident.Verified,
                
                Success = true,
                Message = "Operation successful",
                Media = incident.IncidentMedia?.Select(m => new IncidentMediaResponse
                {
                    Id = m.Id,
                    Url = string.IsNullOrEmpty(m.Url) ? "" : (m.Url.StartsWith("http") ? m.Url : $"{baseUrl}{m.Url}"),
                    CreatedAt = m.CreatedAt
                }).ToList() ?? new List<IncidentMediaResponse>(),
                History = incident.IncidentHistories?.OrderByDescending(h => h.ChangedAt).Select(h => new IncidentHistoryResponse
                {
                    IncidentId = h.IncidentId,
                    StatusId = h.StatusId,
                    Action = h.Action,
                    ChangedAt = h.ChangedAt
                }).ToList() ?? new List<IncidentHistoryResponse>()
            };
        }

        public async Task<List<HeatmapPointResponse>> GetIncidentHeatmapAsync(DateTime? fromDate)
        {
            var filterDate = fromDate ?? DateTime.UtcNow.AddMonths(-1);

           
            string sql = @"
        SELECT 
            ROUND(CAST(L.Latitude AS FLOAT), 3) AS Latitude, 
            ROUND(CAST(L.Longitude AS FLOAT), 3) AS Longitude, 
            COUNT(I.Id) AS Intensity
        FROM Incidents I
        INNER JOIN Locations L ON I.LocationId = L.Id
        WHERE I.CreatedAt >= {0} 
          AND I.DeletedAt IS NULL
        GROUP BY ROUND(CAST(L.Latitude AS FLOAT), 3), ROUND(CAST(L.Longitude AS FLOAT), 3)
        ORDER BY Intensity DESC";

           
            var data = await _context.Database
                .SqlQueryRaw<HeatmapPointResponse>(sql, filterDate)
                .ToListAsync();

            return data;
        }

        public async Task<byte[]> ExportIncidentsToExcelAsync()
        {
            var incidents = await _context.Incidents
                .Include(i => i.Category)
                .Include(i => i.Severity)
                .Include(i => i.Status)
                .Include(i => i.Location)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Incidents Report");

                var headers = new string[] { "ID", "Title", "Category", "Severity", "Status", "Area", "Date" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                }

                int row = 2;
                foreach (var item in incidents)
                {
                    worksheet.Cell(row, 1).Value = item.Id;
                    worksheet.Cell(row, 2).Value = item.Title;
                    worksheet.Cell(row, 3).Value = item.Category?.Name ?? "N/A";
                    worksheet.Cell(row, 4).Value = item.Severity?.Name?? "N/A";
                    worksheet.Cell(row, 5).Value = item.Status?.Name ?? "N/A";
                    worksheet.Cell(row, 6).Value = item.Location?.AreaName ?? "N/A";
                    worksheet.Cell(row, 7).Value = item.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                    row++;
                }

                worksheet.Columns().AdjustToContents(); 

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task<List<IncidentResponse>> GetIncidentsByCheckpointIdAsync(int checkpointId, string lang = "en")
        {
            var baseUrl = "http://localhost:5034";

            var incidents = await _context.Incidents
                .Include(i => i.Category)
                .Include(i => i.Severity)
                .Include(i => i.Status)
                .Include(i => i.Location)
                .Include(i => i.IncidentMedia)
                .Where(i => i.CheckpointId == checkpointId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

           
            return incidents.Select(i => {
                var response = MapToResponse(i, lang);

              
                response.Media = i.IncidentMedia?.Select(m => new IncidentMediaResponse
                {
                    Id = m.Id,
                    Url = $"{baseUrl}{m.Url}",
                    CreatedAt = m.CreatedAt
                }).ToList() ?? new List<IncidentMediaResponse>();

                return response;
            }).ToList();
        }
    }
}
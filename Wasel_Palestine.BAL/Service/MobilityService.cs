using Wasel_Palestine.BAL.DTOs;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wasel_Palestine.DAL.Data; 

namespace Wasel_Palestine.BAL.Service
{
    public class MobilityService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;

        public MobilityService(HttpClient httpClient, ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task<RouteResponseDto> EstimateRouteAsync(double startLat, double startLng, double endLat, double endLng, bool avoidCheckpoints = false)
        {
            var url = $"http://router.project-osrm.org/route/v1/driving/{startLng},{startLat};{endLng},{endLat}?overview=false";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Routing service (OSRM) unavailable. Please check your internet connection.");

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (!json.TryGetProperty("routes", out var routes) || routes.GetArrayLength() == 0)
                throw new Exception("No route found between the specified coordinates.");

            var route = routes[0];
            double distance = route.GetProperty("distance").GetDouble() / 1000; 
            double baseDuration = route.GetProperty("duration").GetDouble() / 60; 

            List<string> factorDetails = new List<string>();
            double totalAdditionalDelay = 0;

           
            var problematicCheckpoints = await _context.Checkpoints
                .Where(c => c.CurrentStatus != "Open")
                .ToListAsync();

            foreach (var cp in problematicCheckpoints)
            {
                if (avoidCheckpoints)
                {
                    totalAdditionalDelay += 120;
                    factorDetails.Add($"[تجنب] تم حساب طريق بديل لتفادي منطقة {cp.NameAr}");
                }
                else
                {
                    double delay = cp.CurrentStatus switch
                    {
                        "Closed" => 60,          
                        "Partially Closed" => 30,  
                        "Busy" => (double)(cp.EstimatedDelayMinutes > 0 ? cp.EstimatedDelayMinutes : 15),
                        _ => 10
                    };

                    totalAdditionalDelay += delay;
                    factorDetails.Add($"{cp.NameAr}: {cp.CurrentStatus} (+{delay} min)");
                }
            }

            var activeIncidents = await _context.Incidents
                .Include(i => i.Status)
                .Include(i => i.Severity)
                .Where(i => i.Verified &&
                           (i.Status.Name == "Active" || i.Status.Name == "Confirmed" || i.Status.Name == "Open"))
                .ToListAsync();

            int incidentCount = 0;
            foreach (var incident in activeIncidents)
            {
                incidentCount++;
                double incidentDelay = incident.Severity?.Name switch
                {
                    "High" => 25,
                    "Medium" => 15,
                    _ => 5
                };

                totalAdditionalDelay += incidentDelay;

                if (incidentCount <= 3)
                {
                    string severityPrefix = incident.Severity?.Name == "High" ? "خطر شديد" : "تنبيه";
                    factorDetails.Add($"{severityPrefix}: {incident.TitleAr}");
                }
            }

            if (incidentCount > 3)
            {
                factorDetails.Add($"+ يوجد {incidentCount - 3} عوائق أخرى تم أخذها في الحسبان");
            }

            return new RouteResponseDto
            {
                DistanceKm = avoidCheckpoints ? Math.Round(distance * 1.4, 2) : Math.Round(distance, 2),

                DurationMinutes = Math.Round(baseDuration + totalAdditionalDelay, 0),

                Remarks = factorDetails.Any()
                          ? string.Join(" | ", factorDetails)
                          : "الطريق سالك حالياً بناءً على البيانات المتوفرة.",

                Metadata = $"Analysis based on {problematicCheckpoints.Count} checkpoints and {activeIncidents.Count} live incidents. AvoidMode: {avoidCheckpoints}"
            };
        }
    }
}
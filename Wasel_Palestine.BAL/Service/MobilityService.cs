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

        public async Task<RouteResponseDto> EstimateRouteAsync(double startLat, double startLng, double endLat, double endLng)
        {
            var url = $"http://router.project-osrm.org/route/v1/driving/{startLng},{startLat};{endLng},{endLat}?overview=false";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Routing service (OSRM) unavailable");

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var route = json.GetProperty("routes")[0];

            double distance = route.GetProperty("distance").GetDouble() / 1000;
            double baseDuration = route.GetProperty("duration").GetDouble() / 60;

            List<string> factorDetails = new List<string>();
            double totalAdditionalDelay = 0;

            var activeCheckpoints = await _context.Checkpoints
                .Where(c => c.CurrentStatus != "Open")
                .ToListAsync();

            foreach (var cp in activeCheckpoints)
            {
                string cpName = cp.NameAr;

                if (cp.CurrentStatus == "Closed")
                {
                    totalAdditionalDelay += 45;
                    factorDetails.Add($"{cpName} مغلق تماماً");
                }
                else if (cp.CurrentStatus == "Partially Closed")
                {
                    totalAdditionalDelay += 25;
                    factorDetails.Add($"{cpName} مفتوح جزئياً");
                }
                else if (cp.CurrentStatus == "Busy")
                {
                    int delay = (int)(cp.EstimatedDelayMinutes > 0 ? cp.EstimatedDelayMinutes : 15);
                    totalAdditionalDelay += delay;
                    factorDetails.Add($"ازدحام مروري عند {cpName}");
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
                double incidentDelay = (incident.Severity?.Name == "High") ? 20 : 10;
                totalAdditionalDelay += incidentDelay;

                if (incidentCount <= 3)
                {
                    string prefix = incident.Severity?.Name == "High" ? "تأخير شديد" : "تنبيه";
                    factorDetails.Add($"{prefix}: {incident.TitleAr}");
                }
            }

            if (incidentCount > 3)
            {
                factorDetails.Add($"+ {incidentCount - 3} بلاغات أخرى عن عوائق في الطريق");
            }

            return new RouteResponseDto
            {
                DistanceKm = Math.Round(distance, 2),
                DurationMinutes = Math.Round(baseDuration + totalAdditionalDelay, 0),
                Remarks = factorDetails.Count > 0
                          ? string.Join(" | ", factorDetails)
                          : "الطريق سالك حالياً بناءً على البيانات المتوفرة.",
                Metadata = $"Analysis based on {activeCheckpoints.Count} checkpoints and {activeIncidents.Count} verified incidents."
            };
        }
    }
}
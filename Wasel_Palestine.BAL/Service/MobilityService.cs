using Wasel_Palestine.BAL.DTOs;
using Wasel_Palestine.DAL.Repository;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json;

namespace Wasel_Palestine.BAL.Service
{
    public class MobilityService
    {
        private readonly HttpClient _httpClient;
        private readonly ICheckpointRepository _checkpointRepository;
        private readonly IMemoryCache _cache; 

        public MobilityService(HttpClient httpClient, ICheckpointRepository checkpointRepository, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _checkpointRepository = checkpointRepository;
            _cache = cache;
        }

        public async Task<RouteResponseDto> EstimateRouteAsync(double startLat, double startLng, double endLat, double endLng)
        {
            double distanceKm = 0;
            double durationMinutes = 0;
            string remarks = "Route is clear.";

            try
            {
               
                var url = $"http://router.project-osrm.org/route/v1/driving/{startLng},{startLat};{endLng},{endLat}?overview=false";
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                    var route = json.GetProperty("routes")[0];
                    distanceKm = route.GetProperty("distance").GetDouble() / 1000;
                    durationMinutes = route.GetProperty("duration").GetDouble() / 60;
                }
                else
                {
                
                    distanceKm = CalculateHaversine(startLat, startLng, endLat, endLng);
                    durationMinutes = distanceKm * 1.5;
                    remarks = "Note: Using estimated data; routing service is currently offline.";
                }

        
                var activeCheckpoints = await _cache.GetOrCreateAsync("active_checkpoints", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    var all = await _checkpointRepository.GetAllCheckpointsAsync();
                    return all.Where(c => c.DeletedAt == null && c.CurrentStatus != "Open").ToList();
                });

                
                var nearbyCheckpoints = activeCheckpoints
                    .Where(c => c.Location != null && (
                        IsNearPoint((double)c.Location.Latitude, (double)c.Location.Longitude, startLat, startLng, 3.0) ||
                        IsNearPoint((double)c.Location.Latitude, (double)c.Location.Longitude, endLat, endLng, 3.0)))
                    .ToList();

                if (nearbyCheckpoints.Any())
                {
                    double checkpointDelay = nearbyCheckpoints.Sum(c => (double)(c.EstimatedDelayMinutes ?? 0));
                    durationMinutes += checkpointDelay;
                    var names = nearbyCheckpoints.Select(c => c.NameAr ?? c.NameEn).ToList();
                 remarks = $"Expected delay at {nearbyCheckpoints.Count} checkpoints: ({string.Join(", ", names)}). " +
               $"Added: {checkpointDelay} minutes.";
                }

                return new RouteResponseDto
                {
                    DistanceKm = Math.Round(distanceKm, 2),
                    DurationMinutes = Math.Round(durationMinutes, 0),
                    Remarks = remarks
                };
            }
            catch (Exception)
            {
              
                return new RouteResponseDto { Remarks = "Service error. Please try again later." };
            }
        }

        private static bool IsNearPoint(double lat1, double lng1, double lat2, double lng2, double thresholdKm)
        {
            return CalculateHaversine(lat1, lng1, lat2, lng2) <= thresholdKm;
        }

        private static double CalculateHaversine(double lat1, double lng1, double lat2, double lng2)
        {
            const double R = 6371;
            double dLat = ToRad(lat2 - lat1);
            double dLng = ToRad(lng2 - lng1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                       Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static double ToRad(double deg) => deg * Math.PI / 180;
    }
}
using Wasel_Palestine.BAL.DTOs;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json;

namespace Wasel_Palestine.BAL.Service
{
    public class MobilityService 
{private readonly HttpClient _httpClient;
 
 public MobilityService(HttpClient httpClient)
   {
       _httpClient=httpClient;     
    }


public async Task<RouteResponseDto> EstimateRouteAsync(double startLat, double startLng, double endLat, double endLng)
{
var url = $"http://router.project-osrm.org/route/v1/driving/{startLng},{startLat};{endLng},{endLat}?overview=false";
var response = await _httpClient.GetAsync(url);
if (!response.IsSuccessStatusCode) throw new Exception("Routing service unavailable");
var json = await response.Content.ReadFromJsonAsync<JsonElement>();
var route =json.GetProperty("routes")[0];

double distance=route.GetProperty("distance").GetDouble()/1000;
double duration=route.GetProperty("duration").GetDouble()/60;

string remarks = "Route is clear";
    if (distance > 5) { 
        duration += 15; 
        remarks = "Potential delays at checkpoints based on current intelligence.";
    }
return new RouteResponseDto {
        DistanceKm = Math.Round(distance, 2),
        DurationMinutes = Math.Round(duration, 0),
        Remarks = remarks
    };

  }
 }
}
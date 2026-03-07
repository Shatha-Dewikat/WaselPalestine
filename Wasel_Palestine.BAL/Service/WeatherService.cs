using Wasel_Palestine.PL.DTOs;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json;


namespace Wasel_Palestine.BAL.Service
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WeatherResponseDto> GetCurrentWeatherAsync(double lat, double lon)
        {
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true";
            
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                using var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("current_weather", out var currentWeather))
                {
                    return new WeatherResponseDto
                    {
                        Latitude = lat,
                        Longitude = lon,
                        Temperature = currentWeather.GetProperty("temperature").GetRawText() + "°C",
                        Condition = "Clear"
                    };
                }
            }

throw new Exception("Failed to connect to the weather service provider.");

        }
    }
}
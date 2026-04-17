using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using Wasel_Palestine.DAL.DTO.Response;

namespace  Wasel_Palestine.BAL.Service
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public WeatherService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<WeatherResponseDto> GetCurrentWeatherAsync(double lat, double lon)
        {
            string cacheKey = $"weather_{Math.Round(lat, 2)}_{Math.Round(lon, 2)}";

           
            if (_cache.TryGetValue(cacheKey, out WeatherResponseDto cachedWeather))
            {
                return cachedWeather;
            }

            var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                using var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("current_weather", out var current))
                    throw new Exception("Weather data not available.");

                var weatherData = new WeatherResponseDto
                {
                    Latitude = lat,
                    Longitude = lon,
                    Temperature = current.GetProperty("temperature").GetRawText() + "°C",
                    Condition = MapWeatherCode(current.GetProperty("weathercode").GetInt32()),
                    LastUpdated = DateTime.UtcNow
                };

                
                _cache.Set(cacheKey, weatherData, TimeSpan.FromMinutes(30));

                return weatherData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Weather service error: {ex.Message}");
            }
        }

        private string MapWeatherCode(int code) => code switch
        {
            0 => "Clear Sky",
            1 or 2 or 3 => "Mainly Clear / Partly Cloudy",
            45 or 48 => "Fog",
            51 or 53 or 55 => "Drizzle",
            61 or 63 or 65 => "Rain",
            71 or 73 or 75 => "Snow Fall",
            77 => "Snow Grains",
            80 or 81 or 82 => "Rain Showers",
            85 or 86 => "Snow Showers",
            95 or 96 or 99 => "Thunderstorm",
            _ => "Unknown Condition"
        };
    }
}
using Microsoft.AspNetCore.Mvc;
using Wasel_Palestine.BAL.Service;

namespace Wasel_Palestine.PL.Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MobilityController : ControllerBase
    {
        private readonly WeatherService _weatherService;
       private readonly MobilityService _mobilityService;
        public MobilityController(WeatherService weatherService, MobilityService mobilityService)
        {
            _weatherService = weatherService;
            _mobilityService = mobilityService;
        }

        [HttpGet("weather")] 
        public async Task<IActionResult> GetWeather([FromQuery] double lat, [FromQuery] double lon)
        {
            var result = await _weatherService.GetCurrentWeatherAsync(lat, lon);
            return Ok(result); 
        }

        [HttpGet("estimate-route")]
public async Task<IActionResult> GetRoute([FromQuery] double sLat, [FromQuery] double sLng, [FromQuery] double eLat, [FromQuery] double eLng)
{
    var route = await _mobilityService.EstimateRouteAsync(sLat, sLng, eLat, eLng);
    return Ok(route);
}
    }
}
using Microsoft.AspNetCore.Mvc;
using Wasel_Palestine.BAL.Service;

namespace Wasel_Palestine.PL.Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MobilityController : ControllerBase
    {
        private readonly WeatherService _weatherService;

        public MobilityController(WeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        [HttpGet("weather")] 
        public async Task<IActionResult> GetWeather([FromQuery] double lat, [FromQuery] double lon)
        {
            var result = await _weatherService.GetCurrentWeatherAsync(lat, lon);
            return Ok(result); 
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Wasel_Palestine.BLL.Service;

namespace Wasel_Palestine.PL.Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MobilityController : ControllerBase
    {
        private readonly IIncidentService _incidentService;
        private readonly IWeatherService _weatherService;

        public MobilityController(IIncidentService incidentService, IWeatherService weatherService)
        {
            _incidentService = incidentService;
            _weatherService = weatherService;
        }

      
        [HttpGet("weather")]
        public async Task<IActionResult> GetWeather([FromQuery] double lat, [FromQuery] double lon)
        {
            var result = await _weatherService.GetCurrentWeatherAsync(lat, lon);
            return Ok(result);
        }

       
        [HttpPost("process-weather-incidents")]
        public async Task<IActionResult> ProcessWeatherIncidents()
        {
            await _incidentService.ProcessWeatherIncidentsAsync();
            return Ok(new { success = true, message = "Weather incidents processed" });
        }
    }
}
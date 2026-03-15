using Microsoft.AspNetCore.Mvc;
using Wasel_Palestine.BAL.Service;
using Wasel_Palestine.BAL.DTOs;

namespace Wasel_Palestine.PL.Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MobilityController : ControllerBase
    {
        private readonly WeatherService _weatherService;
       private readonly MobilityService _mobilityService;
          private readonly ReportingService _reportingService;

        public MobilityController(WeatherService weatherService, MobilityService mobilityService,ReportingService reportingService )
        {
            _weatherService = weatherService;
            _mobilityService = mobilityService;
            _reportingService= reportingService;
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

[HttpPost("submit-report")]
public async Task<IActionResult> PostReport([FromBody] CreateReportDto reportDto)
{
    var result = await _reportingService.SubmitReportAsync(reportDto);
    
    if (result.StartsWith("Success"))
        return Ok(new { message = result });
    
    return Conflict(new { message = result }); // Conflict (409) تعني وجود تكرار
}
    }
}
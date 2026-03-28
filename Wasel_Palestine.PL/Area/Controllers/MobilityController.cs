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

        public MobilityController(WeatherService weatherService, MobilityService mobilityService, ReportingService reportingService)
        {
            _weatherService = weatherService;
            _mobilityService = mobilityService;
            _reportingService = reportingService;
        }

        [HttpGet("weather")]
        public async Task<IActionResult> GetWeather([FromQuery] double lat, [FromQuery] double lon)
        {
            try
            {
                var result = await _weatherService.GetCurrentWeatherAsync(lat, lon);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { message = "Weather service is temporarily unavailable.", error = ex.Message });
            }
        }

        [HttpGet("estimate-route")]
        public async Task<IActionResult> GetRoute([FromQuery] double sLat, [FromQuery] double sLng, [FromQuery] double eLat, [FromQuery] double eLng)
        {
            try
            {
                var route = await _mobilityService.EstimateRouteAsync(sLat, sLng, eLat, eLng);
                return Ok(route);
            }
            catch (Exception ex)
            {
               
                return StatusCode(503, new { message = "Routing service is temporarily unavailable.", error = ex.Message });
            }
        }

        [HttpPost("submit-report")]
        public async Task<IActionResult> PostReport([FromBody] CreateReportDto reportDto)
        {
            try
            {
                var result = await _reportingService.SubmitReportAsync(reportDto);

                if (result.StartsWith("Success"))
                    return Ok(new { message = result });

                return Conflict(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while submitting the report.", error = ex.Message });
            }
        }

        [HttpPost("dismiss-report/{id}")]
        public async Task<IActionResult> DismissReport(int id)
        {
            try
            {
                var result = await _reportingService.DismissReportAsync(id);

                if (result.StartsWith("Error"))
                    return NotFound(new { message = result });

                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while dismissing the report.", error = ex.Message });
            }
        }

        [HttpGet("active-reports")]
        public async Task<IActionResult> GetActiveReports()
        {
            try
            {
                var reports = await _reportingService.GetActiveReportsAsync();
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while fetching active reports.", error = ex.Message });
            }
        }
        [HttpPost("subscribe-alert")]
public async Task<IActionResult> SubscribeAlert([FromBody] SubscribeAlertDto subscriptionDto)
{
    var result = await _reportingService.SubscribeToAlertAsync(subscriptionDto);
    
    if (result.StartsWith("Success"))
        return Ok(new { message = result });

    return BadRequest(new { message = result });
}
    }
}
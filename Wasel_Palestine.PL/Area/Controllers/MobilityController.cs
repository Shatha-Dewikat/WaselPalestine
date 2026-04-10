using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wasel_Palestine.BAL.Service;
using Wasel_Palestine.BAL.DTOs;
using Wasel_Palestine.BLL.Service;

namespace Wasel_Palestine.PL.Area.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MobilityController : ControllerBase
    {
        private readonly IIncidentService _incidentService;
        private readonly IWeatherService _weatherService;
        private readonly WeatherService _weatherService2;
        private readonly MobilityService _mobilityService;
        private readonly ReportingService _reportingService;

        public MobilityController(
            IIncidentService incidentService,
            IWeatherService weatherService,
            WeatherService weatherService2,
            MobilityService mobilityService,
            ReportingService reportingService)
        {
            _incidentService = incidentService;
            _weatherService = weatherService;
            _weatherService2 = weatherService2;
            _mobilityService = mobilityService;
            _reportingService = reportingService;
        }

        
        [HttpGet("weather")]
        [Authorize]
        public async Task<IActionResult> GetWeather([FromQuery] double lat, [FromQuery] double lon)
        {
            try
            {
                var result = await _weatherService.GetCurrentWeatherAsync(lat, lon);
                return Ok(result);
            }
            catch
            {
                return StatusCode(503, new { message = "Weather service unavailable" });
            }
        }

   
        [HttpPost("process-weather-incidents")]
        [Authorize]
        public async Task<IActionResult> ProcessWeatherIncidents()
        {
            await _incidentService.ProcessWeatherIncidentsAsync();
            return Ok(new { success = true });
        }

      
        //[HttpGet("estimate-route")]
        //public async Task<IActionResult> GetRoute([FromQuery] double sLat, [FromQuery] double sLng, [FromQuery] double eLat, [FromQuery] double eLng)
        //{
        //    try
        //    {
        //        var route = await _mobilityService.EstimateRouteAsync(sLat, sLng, eLat, eLng);
        //        return Ok(route);
        //    }
        //    catch
        //    {
        //        return StatusCode(503, new { message = "Routing service unavailable" });
        //    }
        //}

      
        
       
        [HttpPost("dismiss-report/{id}")]
        public async Task<IActionResult> DismissReport(int id)
        {
            var result = await _reportingService.DismissReportAsync(id);

            if (result.StartsWith("Error"))
                return NotFound(new { message = result });

            return Ok(new { message = result });
        }

     
        [HttpGet("active-reports")]
        public async Task<IActionResult> GetActiveReports()
        {
            var reports = await _reportingService.GetActiveReportsAsync();
            return Ok(reports);
        }

       
      
    }
}
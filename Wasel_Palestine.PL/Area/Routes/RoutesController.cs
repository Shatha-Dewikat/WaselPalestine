using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wasel_Palestine.BAL.Service;
using Wasel_Palestine.BAL.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Wasel_Palestine.PL.Area.Routes
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RoutesController : ControllerBase
    {
        private readonly MobilityService _mobilityService;

        public RoutesController(MobilityService mobilityService)
        {
            _mobilityService = mobilityService;
        }

  
        [HttpGet("estimate-route")]
        [Authorize]
        public async Task<IActionResult> GetRoute(
            [FromQuery] double sLat, [FromQuery] double sLng,
            [FromQuery] double eLat, [FromQuery] double eLng)
        {
            if (sLat == 0 || sLng == 0 || eLat == 0 || eLng == 0)
            {
                return BadRequest(new { success = false, message = "Please provide correct start and end coordinates." });
            }

            try
            {
                var routeResult = await _mobilityService.EstimateRouteAsync(sLat, sLng, eLat, eLng);

                return Ok(new
                {
                    success = true,
                    data = routeResult,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    success = false,
                    message = "Route estimation service is currently unavailable. (OSRM Error).",
                    debug_info = ex.Message 
                });
            }
        }
    }
}
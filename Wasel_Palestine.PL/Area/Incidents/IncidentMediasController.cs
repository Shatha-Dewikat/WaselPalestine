using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Threading.Tasks;
using Wasel_Palestine.BLL.Service;
using Wasel_Palestine.DAL.DTO.Request;

namespace Wasel_Palestine.PL.Area.Incidents
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentMediasController : ControllerBase
    {
        private readonly IIncidentMediaService _service;
        public IncidentMediasController(IIncidentMediaService service) => _service = service;

        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        [EnableRateLimiting("strict-by-ip")]
        public async Task<IActionResult> AddMedia([FromForm] IncidentMediaCreateRequest request)
        {
            try
            {
                var result = await _service.AddMediaAsync(request);
                return Ok(new { success = true, message = "Media added successfully.", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> DeleteMedia(int id)
        {
            try
            {
                await _service.DeleteMediaAsync(id);
                return Ok(new { success = true, message = "Media deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpGet("incident/{incidentId}")]
        [EnableRateLimiting("fixed-by-ip")]
        public async Task<IActionResult> GetByIncidentId(int incidentId)
        {
            try
            {
                var result = await _service.GetByIncidentIdAsync(incidentId);
                return Ok(new { success = true, message = "Media retrieved successfully.", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }
    }
}
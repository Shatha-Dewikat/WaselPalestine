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
    public class IncidentStatusesController : ControllerBase
    {
        private readonly IIncidentStatusService _service;

        public IncidentStatusesController(IIncidentStatusService service)
        {
            _service = service;
        }

        private string CurrentUserId => User?.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        private string ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
        private string UserAgent => Request.Headers["User-Agent"].ToString() ?? "N/A";

        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        [EnableRateLimiting("strict-by-ip")]
        public async Task<IActionResult> Create(IncidentStatusCreateRequest request)
        {
            try
            {
                var result = await _service.CreateStatusAsync(request, CurrentUserId, ClientIp, UserAgent);
                return Ok(new
                {
                    success = true,
                    message = $"Status '{result.Name}' created successfully.",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Moderator")]
        [EnableRateLimiting("strict-by-ip")]
        public async Task<IActionResult> Update(int id, IncidentStatusUpdateRequest request)
        {
            try
            {
                var result = await _service.UpdateStatusAsync(id, request, CurrentUserId, ClientIp, UserAgent);
                return Ok(new
                {
                    success = true,
                    message = $"Status '{result.Name}' updated successfully.",
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Moderator")]
        [EnableRateLimiting("strict-by-ip")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteStatusAsync(id, CurrentUserId, ClientIp, UserAgent);
                return Ok(new { success = true, message = $"Status with ID {id} deleted successfully." });
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

        [HttpGet("{id}")]
        [EnableRateLimiting("fixed-by-ip")]

        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetStatusByIdAsync(id);
                if (result == null)
                    return NotFound(new { success = false, message = $"Status with ID {id} not found." });

                return Ok(new { success = true, message = "Status retrieved successfully.", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpGet]
        [EnableRateLimiting("fixed-by-ip")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllStatusesAsync();
                return Ok(new { success = true, message = "All statuses retrieved successfully.", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }
    }
}
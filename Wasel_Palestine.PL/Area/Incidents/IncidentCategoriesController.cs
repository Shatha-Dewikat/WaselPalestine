using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Threading.Tasks;
using Wasel_Palestine.BAL.Service;
using Wasel_Palestine.DAL.DTO.Request;

namespace Wasel_Palestine.PL.Area.Incidents
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class IncidentCategoriesController : ControllerBase
    {
        private readonly IIncidentCategoryService _service;
        public IncidentCategoriesController(IIncidentCategoryService service) => _service = service;

        private string CurrentUserId => User?.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        private string ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
        private string UserAgent => Request.Headers["User-Agent"].ToString() ?? "N/A";

        [HttpPost]
        [Authorize(Roles = "Moderator,Admin")]
        [EnableRateLimiting("strict-by-ip")]
        public async Task<IActionResult> Create([FromBody] IncidentCategoryCreateRequest request)
        {
            try
            {
                var result = await _service.CreateIncidentCategoryAsync(request, CurrentUserId, ClientIp, UserAgent);
                return Ok(new { success = true, message = "Category created successfully.", data = result });
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
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] IncidentCategoryUpdateRequest request)
        {
            try
            {
                var result = await _service.UpdateIncidentCategoryAsync(id, request, CurrentUserId, ClientIp, UserAgent);
                return Ok(new { success = true, message = "Category updated successfully.", data = result });
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
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteIncidentCategoryAsync(id, CurrentUserId, ClientIp, UserAgent);
                return Ok(new { success = true, message = "Category deleted successfully." });
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

        [HttpPut("restore/{id}")]
        [Authorize(Roles = "Moderator,Admin")]
        [EnableRateLimiting("strict-by-ip")]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                await _service.RestoreIncidentCategoryAsync(id, CurrentUserId, ClientIp, UserAgent);
                return Ok(new { success = true, message = "Category restored successfully." });
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
        [Authorize]
        public async Task<IActionResult> GetById(int id, [FromQuery] string lang = "en")
        {
            try
            {
                var category = await _service.GetIncidentCategoryByIdAsync(id, lang);
                if (category == null) return NotFound(new { success = false, message = "Category not found." });
                return Ok(new { success = true, message = "Category retrieved successfully.", data = category });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpGet]
        [Authorize]
        [EnableRateLimiting("fixed-by-ip")]
        public async Task<IActionResult> GetAll([FromQuery] string lang = "en")
        {
            try
            {
                var categories = await _service.GetAllIncidentCategoriesAsync(lang);
                return Ok(new { success = true, message = "All categories retrieved successfully.", data = categories });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }
    }
}
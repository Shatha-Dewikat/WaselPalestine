using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.Tasks;
using Wasel_Palestine.BAL.Service;
using Wasel_Palestine.DAL.DTO.Request;

namespace Wasel_Palestine.PL.Area.Checkpoints
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckpointsController : ControllerBase
    {
        private readonly ICheckpointService _service;

        public CheckpointsController(ICheckpointService service) => _service = service;

        private string CurrentUserId => User?.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

        private (string ip, string userAgent) GetClientInfo() =>
            (HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A",
             Request.Headers["User-Agent"].ToString() ?? "N/A");

        [HttpPost]
        [Authorize(Roles = "Moderator,Admin")]
        [EnableRateLimiting("strict-by-ip")]
        public async Task<IActionResult> Create([FromBody] CreateCheckpointRequest request)
        {
            var (ip, userAgent) = GetClientInfo();
            try
            {
                var result = await _service.CreateCheckpointAsync(request, CurrentUserId, ip, userAgent);
                return Ok(new { success = true, message = "Checkpoint created successfully.", data = result });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error creating checkpoint: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCheckpointRequest request)
        {
            var (ip, userAgent) = GetClientInfo();
            try
            {
                var result = await _service.UpdateCheckpointAsync(id, request, CurrentUserId, ip, userAgent);
                return Ok(new { success = true, message = "Checkpoint updated successfully.", data = result });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error updating checkpoint: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var (ip, userAgent) = GetClientInfo();
            try
            {
                await _service.DeleteCheckpointAsync(id, CurrentUserId, ip, userAgent);
                return Ok(new { success = true, message = "Checkpoint deleted successfully." });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting checkpoint: {ex.Message}" });
            }
        }

        [HttpPost("{id}/restore")]
        [Authorize(Roles = "Moderator,Admin")]
        [EnableRateLimiting("strict-by-ip")]
        public async Task<IActionResult> Restore(int id)
        {
            var (ip, userAgent) = GetClientInfo();
            try
            {
                var result = await _service.RestoreCheckpointAsync(id, CurrentUserId, ip, userAgent);
                return Ok(new { success = true, message = "Checkpoint restored successfully.", data = result });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error restoring checkpoint: {ex.Message}" });
            }
        }

        [HttpPost("{id}/change-status")]
        [Authorize(Roles = "Moderator,Admin")]
        [EnableRateLimiting("strict-by-ip")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeCheckpointStatusRequest request)
        {
            var (ip, userAgent) = GetClientInfo();
            try
            {
                var result = await _service.ChangeStatusAsync(id, request, CurrentUserId, ip, userAgent);
                return Ok(new { success = true, message = "Checkpoint status changed successfully.", data = result });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error changing status: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        [EnableRateLimiting("fixed-by-ip")]
        public async Task<IActionResult> GetById(int id, [FromQuery] string lang = "en")
        {
            try
            {
                var checkpoint = await _service.GetCheckpointByIdAsync(id, lang);
                if (checkpoint == null)
                    return NotFound(new { success = false, message = "Checkpoint not found." });
                return Ok(new { success = true, message = "Checkpoint retrieved successfully.", data = checkpoint });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error retrieving checkpoint: {ex.Message}" });
            }
        }

        [HttpGet]
        [Authorize]
        [EnableRateLimiting("fixed-by-ip")]
        public async Task<IActionResult> GetAll([FromQuery] string lang = "en")
        {
            try
            {
                var result = await _service.GetAllCheckpointsAsync(lang);
                return Ok(new { success = true, message = "All checkpoints retrieved successfully.", data = result });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error retrieving checkpoints: {ex.Message}" });
            }
        }

        [HttpGet("filter")]
        [Authorize]
        [EnableRateLimiting("fixed-by-ip")]
        public async Task<IActionResult> GetFiltered([FromQuery] CheckpointFilterRequest filter, [FromQuery] string lang = "en")
        {
            try
            {
                var result = await _service.GetFilteredCheckpointsAsync(filter, lang);
                return Ok(new { success = true, message = "Filtered checkpoints retrieved successfully.", data = result });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error filtering checkpoints: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        [Authorize]
        [EnableRateLimiting("fixed-by-ip")]
        public async Task<IActionResult> GetPaged([FromQuery] CheckPointPaginationRequest pagination, [FromQuery] string lang = "en")
        {
            if (pagination.PageNumber < 1 || pagination.PageSize < 1)
                return BadRequest(new { success = false, message = "Invalid pagination values." });

            try
            {
                var result = await _service.GetPagedCheckpointsAsync(pagination, lang);
                return Ok(new { success = true, message = "Paged checkpoints retrieved successfully.", data = result });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error retrieving paged checkpoints: {ex.Message}" });
            }
        }

        [HttpGet("{id}/history")]
        [Authorize(Roles = "Moderator,Admin")]
        [EnableRateLimiting("fixed-by-ip")]
        public async Task<IActionResult> GetHistory(int id)
        {
            try
            {
                var result = await _service.GetCheckpointHistoryAsync(id);
                return Ok(new { success = true, message = "Checkpoint history retrieved successfully.", data = result });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error retrieving history: {ex.Message}" });
            }
        }

        [HttpGet("nearby")]
        [Authorize]
        public async Task<IActionResult> GetNearby([FromQuery] double lat, [FromQuery] double lon, [FromQuery] double radius = 10, [FromQuery] string lang = "en")
        {
            try
            {
                var result = await _service.GetNearbyCheckpointsAsync(lat, lon, radius, lang);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
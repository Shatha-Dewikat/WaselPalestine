using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wasel_Palestine.BLL.Service;
using Wasel_Palestine.DAL.DTO.Request;

namespace Wasel_Palestine.PL.Area.Checkpoints
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckpointsController : ControllerBase
    {
        private readonly ICheckpointService _service;

        public CheckpointsController(ICheckpointService service)
        {
            _service = service;
        }

        private string CurrentUserId => User?.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

        [HttpPost]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCheckpointRequest request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
            var userAgent = Request.Headers["User-Agent"].ToString() ?? "N/A";

            var result = await _service.CreateCheckpointAsync(request, CurrentUserId, ip, userAgent);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCheckpointRequest request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
            var userAgent = Request.Headers["User-Agent"].ToString() ?? "N/A";

            var result = await _service.UpdateCheckpointAsync(id, request, CurrentUserId, ip, userAgent);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
            var userAgent = Request.Headers["User-Agent"].ToString() ?? "N/A";

            var result = await _service.DeleteCheckpointAsync(id, CurrentUserId, ip, userAgent);
            return Ok(result);
        }

        [HttpPost("{id}/restore")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> Restore(int id)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
            var userAgent = Request.Headers["User-Agent"].ToString() ?? "N/A";

            var result = await _service.RestoreCheckpointAsync(id, CurrentUserId, ip, userAgent);
            return Ok(result);
        }

        [HttpPost("{id}/change-status")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeCheckpointStatusRequest request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
            var userAgent = Request.Headers["User-Agent"].ToString() ?? "N/A";

            var result = await _service.ChangeStatusAsync(id, request, CurrentUserId, ip, userAgent);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id, [FromQuery] string lang = "en")
        {
            var checkpoint = await _service.GetCheckpointByIdAsync(id, lang);
            return Ok(checkpoint);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] string lang = "en")
        {
            var result = await _service.GetAllCheckpointsAsync(lang);
            return Ok(result);
        }

        [HttpGet("filter")]
        [Authorize]
        public async Task<IActionResult> GetFiltered([FromQuery] CheckpointFilterRequest filter, [FromQuery] string lang = "en")
        {
            var result = await _service.GetFilteredCheckpointsAsync(filter, lang);
            return Ok(result);
        }

        [HttpGet("paged")]
        [Authorize]
        public async Task<IActionResult> GetPaged([FromQuery] CheckPointPaginationRequest pagination, [FromQuery] string lang = "en")
        {
            var result = await _service.GetPagedCheckpointsAsync(pagination, lang);
            return Ok(result);
        }

        [HttpGet("{id}/history")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> GetHistory(int id)
        {
            var result = await _service.GetCheckpointHistoryAsync(id);
            return Ok(result);
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wasel_Palestine.BLL.Service;
using Wasel_Palestine.DAL.DTO.Request;
using System.Threading.Tasks;

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

        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Create(IncidentStatusCreateRequest request)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
                var userAgent = Request.Headers["User-Agent"].ToString() ?? "N/A";

                var result = await _service.CreateStatusAsync(request, userId, ip, userAgent);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id, IncidentStatusUpdateRequest request)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
                var userAgent = Request.Headers["User-Agent"].ToString() ?? "N/A";

                var result = await _service.UpdateStatusAsync(id, request, userId, ip, userAgent);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst("UserId")?.Value;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
            var userAgent = Request.Headers["User-Agent"].ToString() ?? "N/A";

            await _service.DeleteStatusAsync(id, userId, ip, userAgent);
            return Ok(new { message = "Status deleted successfully" });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetStatusByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllStatusesAsync();
            return Ok(result);
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wasel_Palestine.BAL.DTOs;
using Wasel_Palestine.BAL.Service;
using Wasel_Palestine.BLL.Service;
using Wasel_Palestine.DAL.DTO.Request;

namespace Wasel_Palestine.PL.Area.Alerts
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertService _alertService;

        public AlertsController(IAlertService alertService)
        {
            _alertService = alertService;
        }

        [HttpPost]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> Create([FromBody] AlertCreateRequest request)
        {
            var userId = User.FindFirst("UserId")?.Value;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
            var userAgent = Request.Headers["User-Agent"].ToString() ?? "N/A";

            var alert = await _alertService.CreateAlertAsync(request, userId, ip, userAgent);
            return Ok(alert);
        }

        [HttpPut]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> Update([FromBody] AlertUpdateRequest request)
        {
            var userId = User.FindFirst("UserId")?.Value;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
            var userAgent = Request.Headers["User-Agent"].ToString() ?? "N/A";

            var alert = await _alertService.UpdateAlertAsync(request, userId, ip, userAgent);
            return Ok(alert);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
            var userAgent = Request.Headers["User-Agent"].ToString() ?? "N/A";

            await _alertService.DeleteAlertAsync(id, userId, ip, userAgent);
            return Ok(new { message = "Alert deleted successfully." });
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id, [FromQuery] string lang = "en")
        {
            var alert = await _alertService.GetAlertByIdAsync(id, lang);
            if (alert == null) return NotFound(new { message = "Alert not found." });
            return Ok(alert);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] string lang = "en")
        {
            var alerts = await _alertService.GetAllAlertsAsync(lang);
            return Ok(alerts);
        }

        [HttpGet("{id}/history")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> GetAlertHistory(int id)
        {
            var history = await _alertService.GetAlertHistoryAsync(id);
            return Ok(new { success = true, data = history });
        }

        [HttpPost("subscribe")]
        [Authorize]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeAlertDto dto)
        {
            dto.UserId = User.FindFirst("UserId")?.Value; 
            var result = await _alertService.SubscribeToAlertAsync(dto);
            return Ok(new { success = true, message = result });
        }
        [HttpGet("unsubscribe")]
        [AllowAnonymous]
        public async Task<IActionResult> Unsubscribe([FromQuery] int alertId, [FromQuery] string userId)
        {
            var result = await _alertService.UnsubscribeFromAlertsAsync(alertId, userId);
            if (!result) return BadRequest("لم يتم العثور على اشتراك نشط.");

            return Content("<h2 style='color:orange;'>تم إلغاء الاشتراك بنجاح</h2>", "text/html; charset=utf-8");
        }

        [HttpGet("alerts-stats")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> GetAlertsStats()
        {
            var stats = await _alertService.GetAlertsStatisticsAsync();
            return Ok(new { success = true, data = stats });
        }


    }
}

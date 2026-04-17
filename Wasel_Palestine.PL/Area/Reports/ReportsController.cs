using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Wasel_Palestine.BAL.Service;
using Wasel_Palestine.DAL.DTO.Request;

namespace Wasel_Palestine.PL.Area.Reports
{
    [Route("api/v1/[controller]")]
    [ApiController]

    public class ReportsController : ControllerBase
    {
        private readonly IIncidentService _incidentService;
        private readonly ReportingService _reportingService; 

        public ReportsController(IIncidentService incidentService, ReportingService reportingService)
        {
            _incidentService = incidentService;
            _reportingService = reportingService; 
        }

        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportIncidents()
        {
            var fileContents = await _incidentService.ExportIncidentsToExcelAsync();
            var fileName = $"Incidents_Report_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

        [HttpDelete("delete-report/{id}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> DeleteReport(int id, [FromQuery] string reason)
        {
            var adminId = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(reason))
                return BadRequest(new { success = false, message = "Please provide a reason for deletion" });

            var result = await _reportingService.DeleteReportAsync(id, adminId, reason);

            if (result == "Report not found")
                return NotFound(new { success = false, message = result });

            return Ok(new { success = true, message = result });
        }

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = await _incidentService.GetDashboardStatsAsync();
            return Ok(new { success = true, data = stats });
        }

        [HttpGet("heatmap")]
        public async Task<IActionResult> GetHeatmap([FromQuery] DateTime? fromDate)
        {
            var heatmapData = await _incidentService.GetIncidentHeatmapAsync(fromDate);
            return Ok(new { success = true, data = heatmapData });
        }

        [HttpPost("submit")]
        [Authorize]
        public async Task<IActionResult> SubmitReport([FromBody] CreateReportDto reportDto)
        {
            try
            {
                reportDto.UserId = User.FindFirst("UserId")?.Value;

                bool isStaff = User.IsInRole("Moderator") || User.IsInRole("Admin");

                var result = await _reportingService.SubmitReportAsync(reportDto, isStaff);

                return Ok(new { success = true, message = result });
            }
            catch (Exception ex)
            {
                

                return StatusCode(500, new
                {
                    success = false,
                    message = "Something went wrong on our end. Please try again later."
                });
            }
        }

        [HttpPost("{id}/vote")]
        [Authorize]
        public async Task<IActionResult> Vote(int id, [FromQuery] bool isUpvote)
        {
            var userId = User.FindFirst("UserId")?.Value;
            var result = await _reportingService.VoteOnReportAsync(id, userId, isUpvote);
            return Ok(new { success = true, message = result });
        }


        [HttpPut("update-status/{id}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] int statusId, [FromQuery] string notes)
        {
            var moderatorId = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(notes))
                return BadRequest(new { success = false, message = "Please provide notes/reason for status change" });

            var result = await _reportingService.UpdateReportStatusAsync(id, statusId, moderatorId, notes);

            if (result.Contains("not found") || result.Contains("Invalid"))
                return NotFound(new { success = false, message = result });

            return Ok(new { success = true, message = result });
        }

        [HttpPost("{id}/upload-media")]
        [Authorize]
        public async Task<IActionResult> UploadMedia(int id, IFormFile file)
        {
            var userId = User.FindFirst("UserId")?.Value;

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var result = await _reportingService.UploadReportMediaAsync(id, userId, file);

            if (result == "Report not found") return NotFound(result);

            return Ok(new { success = true, message = result });
        }

        [HttpGet("mark-as-read")]
        [AllowAnonymous]
        public async Task<IActionResult> MarkAsRead([FromQuery] int alertId, [FromQuery] string userId)
        {
            var success = await _reportingService.MarkAlertAsReadAsync(alertId, userId);

            if (!success)
            {
                return BadRequest("تعذر تحديث الحالة. تأكد من صحة البيانات.");
            }

            return Content("<h3>تمت العملية بنجاح! تم تسجيل قراءة التنبيه.</h3>", "text/html; charset=utf-8");
        }
    }
}
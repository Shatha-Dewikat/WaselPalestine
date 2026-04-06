using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wasel_Palestine.BAL.DTOs;
using Wasel_Palestine.BAL.Service;
using Wasel_Palestine.BLL.Service;

namespace Wasel_Palestine.PL.Area.Reports
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Moderator")]
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
            reportDto.UserId = User.FindFirst("UserId")?.Value;

            bool isStaff = User.IsInRole("Moderator") || User.IsInRole("Admin");

            var result = await _reportingService.SubmitReportAsync(reportDto, isStaff);

            return Ok(new { success = true, message = result });
        }
    }
}
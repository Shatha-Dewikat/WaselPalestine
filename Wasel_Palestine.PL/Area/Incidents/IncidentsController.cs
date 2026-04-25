using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Wasel_Palestine.BLL.Service;
using Wasel_Palestine.DAL.DTO.Request;

namespace Wasel_Palestine.PL.Area.Incidents
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class IncidentsController : ControllerBase
    {
        private readonly IIncidentService _incidentService;
        public IncidentsController(IIncidentService incidentService) => _incidentService = incidentService;

        private string CurrentUserId => User?.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

        [HttpPost]
        [Authorize(Roles = "Moderator,Admin")]
        [EnableRateLimiting("strict-by-ip")]
        public async Task<IActionResult> CreateIncident([FromBody] CreateIncidentRequest request)
        {
            
                var result = await _incidentService.CreateIncidentAsync(request, CurrentUserId);
                if (!result.Success) return BadRequest(new { success = false, message = result.Message, errors = result.Errors });
                return Ok(new { success = true, message = "Incident created and checkpoint status updated.", data = result });
            
           
        }

        [HttpGet("heatmap")]
        [Authorize]
        [EnableRateLimiting("fixed-by-ip")]
        public async Task<IActionResult> GetHeatmap([FromQuery] DateTime? fromDate)
        {
           
            var stats = await _incidentService.GetIncidentHeatmapAsync(fromDate);
            return Ok(new { success = true, data = stats });
        }

        [HttpGet("export")]
        [Authorize(Roles = "Admin,Moderator")]
        [EnableRateLimiting("fixed-by-ip")]
        public async Task<IActionResult> ExportToExcel()
        {
            var fileContents = await _incidentService.ExportIncidentsToExcelAsync();

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Incidents_Report_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> UpdateIncident(int id, [FromBody] UpdateIncidentRequest request)
        {
            
                var result = await _incidentService.UpdateIncidentAsync(id, request, CurrentUserId);
                if (!result.Success) return NotFound(new { success = false, message = result.Message, errors = result.Errors });
                return Ok(new { success = true, message = "Incident updated successfully.", data = result });
            
          
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> DeleteIncident(int id)
        {
            try
            {
                var result = await _incidentService.DeleteIncidentAsync(id, CurrentUserId);
                if (!result.Success) return NotFound(new { success = false, message = result.Message });
                return Ok(new { success = true, message = "Incident deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        [EnableRateLimiting("fixed-by-ip")]
        public async Task<IActionResult> GetIncidentById(int id, [FromQuery] string lang = "en")
        {
            try
            {
                var incident = await _incidentService.GetIncidentByIdAsync(id, lang);
                if (!incident.Success) return NotFound(new { success = false, message = "Incident not found." });
                return Ok(new { success = true, data = incident });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpGet]
        [Authorize]
        [EnableRateLimiting("fixed-by-ip")]
        public async Task<IActionResult> GetAllIncidents([FromQuery] string lang = "en")
        {
            try
            {
                var incidents = await _incidentService.GetIncidentAllAsync(lang);
                return Ok(new { success = true, data = incidents });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpGet("filter")]
        [Authorize]
        [EnableRateLimiting("fixed-by-ip")]
        public async Task<IActionResult> GetFilteredIncidents([FromQuery] IncidentFilterRequest filter, [FromQuery] string lang = "en")
        {
            try
            {
                var incidents = await _incidentService.GetFilteredIncidentsAsync(filter, lang);
                return Ok(new { success = true, data = incidents });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        [Authorize]
        [EnableRateLimiting("fixed-by-ip")]
        public async Task<IActionResult> GetPagedIncidents([FromQuery] PaginationRequest paginationRequest, [FromQuery] string lang = "en")
        {
            if (paginationRequest.PageNumber < 1 || paginationRequest.PageSize < 1)
                return BadRequest(new { success = false, message = "Invalid pagination values." });

            try
            {
                var incidents = await _incidentService.GetPagedIncidentsAsync(paginationRequest, lang);
                return Ok(new { success = true, data = incidents });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpGet("{id}/history")]
        [Authorize]
        [EnableRateLimiting("fixed-by-ip")]
        public async Task<IActionResult> GetIncidentHistory(int id)
        {
            try
            {
                var history = await _incidentService.GetIncidentHistoryAsync(id);
                return Ok(new { success = true, data = history });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpPost("{id}/verify")]
        [Authorize(Roles = "Moderator,Admin")]
        [EnableRateLimiting("strict-by-ip")]
        public async Task<IActionResult> VerifyIncident(int id)
        {
            try
            {
                var result = await _incidentService.VerifyIncidentAsync(id, CurrentUserId);
                if (!result.Success) return BadRequest(new { success = false, message = result.Message });
                return Ok(new { success = true, message = "Incident verified successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpPost("{id}/resolve")]
        [Authorize(Roles = "Moderator,Admin")]
        [EnableRateLimiting("strict-by-ip")]
        public async Task<IActionResult> ResolveIncident(int id)
        {
            try
            {
                var result = await _incidentService.ResolveIncidentAsync(id, CurrentUserId);
                if (!result.Success) return BadRequest(new { success = false, message = result.Message });
                return Ok(new { success = true, message = "Incident resolved successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpPost("{id}/close")]
        [Authorize(Roles = "Moderator,Admin")]
        [EnableRateLimiting("strict-by-ip")]
        public async Task<IActionResult> CloseIncident(int id)
        {
            try
            {
                var result = await _incidentService.CloseIncidentAsync(id, CurrentUserId);
                if (!result.Success) return BadRequest(new { success = false, message = result.Message });
                return Ok(new { success = true, message = "Incident closed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpGet("checkpoint/{checkpointId}")]
        [Authorize]
        [EnableRateLimiting("fixed-by-ip")]
        public async Task<IActionResult> GetIncidentsByCheckpoint(int checkpointId, [FromQuery] string lang = "en")
        {
            try
            {
                var incidents = await _incidentService.GetIncidentsByCheckpointIdAsync(checkpointId, lang);
                return Ok(new { success = true, message = $"Incidents for checkpoint {checkpointId} retrieved.", data = incidents });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpGet("dashboard-stats")]
        [Authorize(Roles = "Admin,Moderator")]
        [EnableRateLimiting("fixed-by-ip")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var stats = await _incidentService.GetDashboardStatsAsync();
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("filtered-paged")]
        [Authorize]
        [EnableRateLimiting("fixed-by-ip")]
        [HttpPost("filtered-paged")] 
        [Authorize]
        public async Task<IActionResult> GetFilteredPagedIncidents([FromBody] IncidentQueryRequest request, [FromQuery] string lang = "en")
        {
            if (request == null) return BadRequest("Request body is empty");

            
            request.Pagination ??= new PaginationRequest { PageNumber = 1, PageSize = 10 };
            request.Filter ??= new IncidentFilterRequest();

            try
            {
                var incidents = await _incidentService.GetFilteredPagedIncidentsAsync(request, lang);
                return Ok(new { success = true, data = incidents });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
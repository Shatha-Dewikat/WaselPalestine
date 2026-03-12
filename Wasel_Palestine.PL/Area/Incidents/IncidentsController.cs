using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Wasel_Palestine.BLL.Service;
using Wasel_Palestine.DAL.DTO.Request;

namespace Wasel_Palestine.PL.Area.Incidents
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentsController : ControllerBase
    {
        private readonly IIncidentService _incidentService;

        public IncidentsController(IIncidentService incidentService)
        {
            _incidentService = incidentService;
        }

        private string CurrentUserId => User?.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

       
        [HttpPost]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> CreateIncident([FromBody] CreateIncidentRequest request)
        {
            var result = await _incidentService.CreateIncidentAsync(request, CurrentUserId);
            return Ok(result);
        }

        
        [HttpPut("{id}")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> UpdateIncident(int id, [FromBody] UpdateIncidentRequest request)
        {
            try
            {
                var result = await _incidentService.UpdateIncidentAsync(id, request, CurrentUserId);
                if (!result.Success) return NotFound(new { message = result.Message });
                return Ok(result);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the incident." });
            }
        }

        
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetIncidentById(int id, [FromQuery] string lang = "en")
        {
            var incident = await _incidentService.GetIncidentByIdAsync(id, lang);
            if (incident == null || !incident.Success) return NotFound(new { message = "Incident not found." });
            return Ok(incident);
        }

       
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllIncidents([FromQuery] string lang = "en")
        {
            var incidents = await _incidentService.GetIncidentAllAsync(lang);
            return Ok(incidents);
        }

        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> DeleteIncident(int id)
        {
            var result = await _incidentService.DeleteIncidentAsync(id, CurrentUserId);
            if (!result.Success) return NotFound(new { message = result.Message });
            return Ok(result);
        }

        
        [HttpGet("filter")]
        [Authorize]
        public async Task<IActionResult> GetFilteredIncidents([FromQuery] IncidentFilterRequest filter, [FromQuery] string lang = "en")
        {
            var incidents = await _incidentService.GetFilteredIncidentsAsync(filter, lang);
            return Ok(incidents);
        }

      
        [HttpGet("paged")]
        [Authorize]
        public async Task<IActionResult> GetPagedIncidents([FromQuery] PaginationRequest paginationRequest, [FromQuery] string lang = "en")
        {
            if (paginationRequest.PageNumber < 1 || paginationRequest.PageSize < 1)
                return BadRequest(new { message = "Invalid pagination values" });

            var incidents = await _incidentService.GetPagedIncidentsAsync(paginationRequest, lang);
            return Ok(incidents);
        }

        
        [HttpGet("query")]
        [Authorize]
        public async Task<IActionResult> GetIncidents([FromQuery] IncidentQueryRequest request, [FromQuery] string lang = "en")
        {
            var incidents = await _incidentService.GetFilteredPagedIncidentsAsync(request, lang);
            return Ok(incidents);
        }

        
        [HttpGet("{id}/history")]
        [Authorize]
        public async Task<IActionResult> GetIncidentHistory(int id)
        {
            var history = await _incidentService.GetIncidentHistoryAsync(id);
            return Ok(history);
        }
    }
}
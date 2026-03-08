using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wasel_Palestine.BLL.Service;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.PL.Area.Incidents
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentsController : ControllerBase
    {
        private readonly IIncidentService _IncidentSevice;

        public IncidentsController(IIncidentService IncidentSevice)
        {
            _IncidentSevice = IncidentSevice;
        }

        [HttpPost]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> CreateIncident(CreateIncidentRequest request)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var result = await _IncidentSevice.CreateIncidentAsync(request, userId);
            return Ok(result);

        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> UpdateIncident(int id, [FromBody] UpdateIncidentRequest request)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value; 
                var updatedIncident = await _IncidentSevice.UpdateIncidentAsync(id, request, userId); 
                return Ok(updatedIncident);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetIncidentById(int id, [FromQuery] string lang = "en")
        {
            var getIncident = await _IncidentSevice.GetIncidentByIdAsync(id, lang);
            if (getIncident == null || !getIncident.Success)
                return NotFound(new { message = "Incident not found." });
            return Ok(getIncident);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllIncidents([FromQuery] string lang = "en")
        {
            var getAllIncidents = await _IncidentSevice.GetIncidentAllAsync(lang);
            return Ok(getAllIncidents);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> DeleteIncident(int id)
        {
            var userId = User.FindFirst("UserId")?.Value; 
            var deletedIncident = await _IncidentSevice.DeleteIncidentAsync(id, userId); 

            if (!deletedIncident.Success)
                return NotFound(new { message = deletedIncident.Message });

            return Ok(deletedIncident);
        }
        [HttpGet("filter")]
        [Authorize]
        public async Task<IActionResult> GetFilteredIncidents(IncidentFilterRequest filter, [FromQuery] string lang = "en")
        {
            var filteredIncidents = await _IncidentSevice.GetFilteredIncidentsAsync(filter, lang);
            return Ok(filteredIncidents);
        }

        [HttpGet("paged")]
        [Authorize]
        public async Task<IActionResult> GetPagedIncidents([FromQuery] PaginationRequest paginationRequest, [FromQuery] string lang = "en")
        {
            if (paginationRequest.PageNumber < 1 || paginationRequest.PageSize < 1)
                return BadRequest("Invalid pagination values");

            var pagedIncidents = await _IncidentSevice.GetPagedIncidentsAsync(paginationRequest, lang);
            return Ok(pagedIncidents);
        }

        [HttpGet("query")]
        [Authorize]
        public async Task<IActionResult> GetIncidents([FromQuery] IncidentQueryRequest request, [FromQuery] string lang = "en")
        {
            var result = await _IncidentSevice.GetFilteredPagedIncidentsAsync(request, lang);
            return Ok(result);
        }
    }
}

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
        public async Task<IActionResult> UpdateIncident(int id, UpdateIncidentRequest request)
        {
            try
            {
                var updatedIncident = await _IncidentSevice.UpdateIncidentAsync(id, request);
                return Ok(updatedIncident);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetIncidentById(int id)
        {
            var getIncident = await _IncidentSevice.GetIncidentByIdAsync(id);
            if (getIncident == null)
                return NotFound(new { message = "Incident not found." });
            return Ok(getIncident);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllIncidents()
        {
            var getAllIncidents = await _IncidentSevice.GetIncidentAllAsync();
            return Ok(getAllIncidents);

        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> DeleteIncident(int id)
        {
            var deletedIncident = await _IncidentSevice.DeleteIncidentAsync(id);
            if (!deletedIncident.Success)
                return NotFound(new { message = deletedIncident.Message });

            return Ok(deletedIncident);
        }
        [HttpGet("filter")]
        [Authorize]
        public async Task<IActionResult> GetFilteredIncidents(IncidentFilterRequest filter)
        {
            var filteredIncidents = await _IncidentSevice.GetFilteredIncidentsAsync(filter);
            return Ok(filteredIncidents);

        }
        [HttpGet("paged")]
        [Authorize]
        public async Task<IActionResult> GetPagedIncidents([FromQuery] PaginationRequest paginationRequest)
        {
            if (paginationRequest.PageNumber < 1 || paginationRequest.PageSize < 1)
                return BadRequest("Invalid pagination values");

            var pagedIncidents = await _IncidentSevice.GetPagedIncidentsAsync(paginationRequest);

            return Ok(pagedIncidents);
        }

        [HttpGet("query")]
        [Authorize]
        public async Task<IActionResult> GetIncidents([FromQuery] IncidentQueryRequest request)
        {
            var result = await _IncidentSevice.GetFilteredPagedIncidentsAsync(request);
            return Ok(result);
        }
    }
}

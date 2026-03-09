using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wasel_Palestine.BLL.Service;
using Wasel_Palestine.DAL.DTO.Request;

namespace Wasel_Palestine.PL.Area.Incidents
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentSeveritiesController : ControllerBase
    {
        private readonly IIncidentSeverityService _service;

        public IncidentSeveritiesController(IIncidentSeverityService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Create(
            IncidentSeverityCreateRequest request)
        {
            var userId = User.FindFirst("UserId")?.Value;

            var result = await _service.CreateIncidentSeverityAsync(request, userId);

            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(
            int id,
            IncidentSeverityUpdateRequest request)
        {
            var userId = User.FindFirst("UserId")?.Value;

            var result = await _service.UpdateIncidentSeverityAsync(id, request, userId);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst("UserId")?.Value;

            await _service.DeleteIncidentSeverityAsync(id, userId);

            return Ok(new { message = "Severity deleted successfully" });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetIncidentSeverityByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllIncidentSeveritiesAsync();

            return Ok(result);
        }
    }
}

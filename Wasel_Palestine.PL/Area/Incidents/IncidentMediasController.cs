using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wasel_Palestine.BLL.Service;
using Wasel_Palestine.DAL.DTO.Request;

namespace Wasel_Palestine.PL.Area.Incidents
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentMediasController : ControllerBase
    {
        private readonly IIncidentMediaService _service;

        public IncidentMediasController(IIncidentMediaService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> AddMedia([FromForm] IncidentMediaCreateRequest request)
        {
            var result = await _service.AddMediaAsync(request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> DeleteMedia(int id)
        {
            await _service.DeleteMediaAsync(id);
            return Ok(new { message = "Media deleted successfully" });
        }

        [HttpGet("incident/{incidentId}")]
        public async Task<IActionResult> GetByIncidentId(int incidentId)
        {
            var result = await _service.GetByIncidentIdAsync(incidentId);
            return Ok(result);
        }
    }
}

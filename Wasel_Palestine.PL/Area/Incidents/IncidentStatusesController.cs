using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wasel_Palestine.BLL.Service;
using Wasel_Palestine.DAL.DTO.Request;

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
       // [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Create(
            IncidentStatusCreateRequest request)
        {
            var userId = User.FindFirst("UserId")?.Value;

            var result = await _service.CreateStatusAsync(request, userId);

            return Ok(result);
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(
            int id,
            IncidentStatusUpdateRequest request)
        {
            var userId = User.FindFirst("UserId")?.Value;

            var result = await _service.UpdateStatusAsync(id, request, userId);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst("UserId")?.Value;

            await _service.DeleteStatusAsync(id, userId);

            return Ok(new { message = "Status deleted successfully" });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetStatusByIdAsync(id);

            if (result == null)
                return NotFound();

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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Wasel_Palestine.BLL.Service;
using Wasel_Palestine.DAL.DTO.Request;

namespace Wasel_Palestine.PL.Area.Incidents
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentCategoriesController : ControllerBase
    {
        private readonly IIncidentCategoryService _incidentCategoryService;

        public IncidentCategoriesController(IIncidentCategoryService incidentCategoryService)
        {
            _incidentCategoryService = incidentCategoryService;
        }

        [HttpPost]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> Create([FromBody] IncidentCategoryCreateRequest request)
        {
            var userId = User.FindFirst("UserId")?.Value;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
            var userAgent = Request.Headers["User-Agent"].ToString() ?? "N/A";

            var category = await _incidentCategoryService.CreateIncidentCategoryAsync(request, userId, ip, userAgent);
            return Ok(category);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] IncidentCategoryUpdateRequest request)
        {
            var userId = User.FindFirst("UserId")?.Value;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
            var userAgent = Request.Headers["User-Agent"].ToString() ?? "N/A";

            var category = await _incidentCategoryService.UpdateIncidentCategoryAsync(id, request, userId, ip, userAgent);
            return Ok(category);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst("UserId")?.Value;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
            var userAgent = Request.Headers["User-Agent"].ToString() ?? "N/A";

            await _incidentCategoryService.DeleteIncidentCategoryAsync(id, userId, ip, userAgent);
            return Ok(new { message = "Category deleted successfully." });
        }

        [HttpPut("restore/{id}")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> Restore(int id)
        {
            var userId = User.FindFirst("UserId")?.Value;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
            var userAgent = Request.Headers["User-Agent"].ToString() ?? "N/A";

            await _incidentCategoryService.RestoreIncidentCategoryAsync(id, userId, ip, userAgent);
            return Ok(new { message = "Category restored successfully." });
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id, [FromQuery] string lang = "en")
        {
            var category = await _incidentCategoryService.GetIncidentCategoryByIdAsync(id, lang);
            if (category == null) return NotFound(new { message = "Category not found." });
            return Ok(category);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] string lang = "en")
        {
            var categories = await _incidentCategoryService.GetAllIncidentCategoriesAsync(lang);
            return Ok(categories);
        }
    }
}
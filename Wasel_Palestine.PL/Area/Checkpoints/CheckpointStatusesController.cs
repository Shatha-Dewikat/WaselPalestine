using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.Tasks;
using Wasel_Palestine.BLL.Service;
using Wasel_Palestine.DAL.DTO.Request;

namespace Wasel_Palestine.PL.Area.Checkpoints
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CheckpointStatusesController : ControllerBase
    {
        private readonly ICheckpointStatusService _service;

        public CheckpointStatusesController(ICheckpointStatusService service) => _service = service;

        [HttpGet]
        [Authorize]
        [EnableRateLimiting("fixed-by-ip")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(new { success = true, message = "All checkpoint statuses retrieved successfully.", data = result });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error retrieving statuses: {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Moderator,Admin")]
        [EnableRateLimiting("strict-by-ip")]
        public async Task<IActionResult> Create([FromBody] CreateCheckpointStatusRequest request)
        {
            try
            {
                var result = await _service.CreateAsync(request);
                if (!result.Success) return BadRequest(new { success = false, message = result.Message });
                return Ok(new { success = true, message = "Checkpoint status created successfully.", data = result });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error creating status: {ex.Message}" });
            }
        }
    }
}
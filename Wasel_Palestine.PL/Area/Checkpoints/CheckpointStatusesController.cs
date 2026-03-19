using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wasel_Palestine.BLL.Service;
using Wasel_Palestine.DAL.DTO.Request;

namespace Wasel_Palestine.PL.Area.Checkpoints
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckpointStatusesController : ControllerBase
    {
        private readonly ICheckpointStatusService _service;

        public CheckpointStatusesController(ICheckpointStatusService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCheckpointStatusRequest request)
        {
            var result = await _service.CreateAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}

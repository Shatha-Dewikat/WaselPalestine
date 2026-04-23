using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wasel_Palestine.BAL.Service;
using Wasel_Palestine.DAL.DTO.Request;

[Route("api/v1/[controller]")]
[ApiController]
public class IncidentMediasController : ControllerBase
{
    private readonly IIncidentMediaService _service;
    public IncidentMediasController(IIncidentMediaService service) => _service = service;

    [HttpPost]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> AddMedia([FromForm] IncidentMediaCreateRequest request)
    {
        try
        {
            var result = await _service.AddMediaAsync(request);
          
            return Ok(new { success = true, message = "Media added successfully.", data = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("incident/{incidentId}")]
    public async Task<IActionResult> GetByIncidentId(int incidentId)
    {
        var result = await _service.GetByIncidentIdAsync(incidentId);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> DeleteMedia(int id)
    {
        try
        {
            await _service.DeleteMediaAsync(id);
            return Ok(new { success = true, message = "تم الحذف بنجاح" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = "فشل الحذف: " + ex.Message });
        }
    }
}
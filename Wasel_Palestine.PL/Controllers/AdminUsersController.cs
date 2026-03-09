using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Utils;

namespace Wasel_Palestine.PL.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminUsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly AuditLogger _audit;

        public AdminUsersController(UserManager<User> userManager, AuditLogger audit)
        {
            _userManager = userManager;
            _audit = audit;
        }

        // POST: /api/admin/users/{userId}/roles/{roleName}
        [HttpPost("{userId}/roles/{roleName}")]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            var res = await _userManager.AddToRoleAsync(user, roleName);
            if (!res.Succeeded) return BadRequest(res.Errors);

            await _audit.LogAsync(userId, "ASSIGN_ROLE", "UserRoles", 0, $"Assigned role: {roleName}", GetIp(), GetUA());
            return Ok(new { message = $"Role '{roleName}' assigned" });
        }

        // POST: /api/admin/users/{userId}/deactivate
        [HttpPost("{userId}/deactivate")]
        public async Task<IActionResult> Deactivate(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            await _audit.LogAsync(userId, "DEACTIVATE_USER", "Users", 0, "User deactivated", GetIp(), GetUA());
            return Ok(new { message = "User deactivated" });
        }

        private string GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        private string GetUA() => Request.Headers.UserAgent.ToString();
    }
}
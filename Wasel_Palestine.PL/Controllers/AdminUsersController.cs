using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // GET: /api/admin/users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.UserName,
                    u.FullName,
                    u.IsActive
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: /api/admin/users/{userId}/roles
        [HttpGet("{userId}/roles")]
        public async Task<IActionResult> GetRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }

        // POST: /api/admin/users/{userId}/roles/{roleName}
        [HttpPost("{userId}/roles/{roleName}")]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            var res = await _userManager.AddToRoleAsync(user, roleName);
            if (!res.Succeeded) return BadRequest(res.Errors);

            await _audit.LogAsync(userId, "ASSIGN_ROLE", "UserRoles", 0,
                $"Assigned role: {roleName}", GetIp(), GetUA());

            return Ok(new { message = $"Role '{roleName}' assigned" });
        }

        // DELETE: /api/admin/users/{userId}/roles/{roleName}
        [HttpDelete("{userId}/roles/{roleName}")]
        public async Task<IActionResult> RemoveRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            var res = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!res.Succeeded) return BadRequest(res.Errors);

            await _audit.LogAsync(userId, "REMOVE_ROLE", "UserRoles", 0,
                $"Removed role: {roleName}", GetIp(), GetUA());

            return Ok(new { message = $"Role '{roleName}' removed" });
        }

        // POST: /api/admin/users/{userId}/deactivate
        [HttpPost("{userId}/deactivate")]
        public async Task<IActionResult> Deactivate(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            await _audit.LogAsync(userId, "DEACTIVATE_USER", "Users", 0,
                "User deactivated", GetIp(), GetUA());

            return Ok(new { message = "User deactivated" });
        }

        // POST: /api/admin/users/{userId}/activate
        [HttpPost("{userId}/activate")]
        public async Task<IActionResult> Activate(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            await _audit.LogAsync(userId, "ACTIVATE_USER", "Users", 0,
                "User activated", GetIp(), GetUA());

            return Ok(new { message = "User activated" });
        }

        private string GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        private string GetUA() => Request.Headers.UserAgent.ToString();
    }
}
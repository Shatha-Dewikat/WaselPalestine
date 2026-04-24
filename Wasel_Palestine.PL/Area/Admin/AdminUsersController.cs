using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Utils;

namespace Wasel_Palestine.PL.Area.Admin
{
    [ApiController]
    [Route("api/admin/v1/users")]
    [Authorize(Policy = "AdminOrModerator")]
    public class AdminUsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly AuditLogger _audit;
        private readonly ApplicationDbContext _db;   

      
        public AdminUsersController(UserManager<User> userManager, AuditLogger audit, ApplicationDbContext db)
        {
            _userManager = userManager;
            _audit = audit;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var total = await _userManager.Users.CountAsync();
            var users = await _userManager.Users
                .OrderBy(u => u.Id) // ضروري قبل Skip
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new { u.Id, u.Email, u.UserName, u.FullName, u.IsActive })
                .ToListAsync();

            return Ok(new { total, page, pageSize, users });
        }
        [HttpGet("{userId}/roles")]
        public async Task<IActionResult> GetRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }

        
[HttpGet("{userId}/lockout")]
public async Task<IActionResult> GetLockoutStatus(string userId)
{
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null) return NotFound("User not found");

    var locked = await _userManager.IsLockedOutAsync(user);

    return Ok(new
    {
        user.Id,
        user.Email,
        lockedOut = locked,
        lockoutEnd = user.LockoutEnd,
        accessFailedCount = user.AccessFailedCount
    });
}

   
[HttpPost("{userId}/unlock")]
public async Task<IActionResult> UnlockUser(string userId)
{
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null) return NotFound("User not found");

    await _userManager.SetLockoutEndDateAsync(user, null);
    await _userManager.ResetAccessFailedCountAsync(user);

    await _audit.LogAsync(userId, "UNLOCK_USER", "Users", 0,
        "Admin unlocked user + reset failed count", GetIp(), GetUA());

    return Ok(new { message = "User unlocked" });
}

        [HttpPost("{userId}/roles/{roleName}")]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

          
            var roleExists = await _db.Roles.AnyAsync(r => r.Name == roleName);
            if (!roleExists)
            {
                return BadRequest(new { message = $"Role '{roleName}' does not exist." });
            }

            try
            {
                var res = await _userManager.AddToRoleAsync(user, roleName);
                if (!res.Succeeded) return BadRequest(res.Errors);

                await _audit.LogAsync(userId, "ASSIGN_ROLE", "UserRoles", 0,
                    $"Assigned role: {roleName}", GetIp(), GetUA());

                return Ok(new { message = $"Role '{roleName}' assigned" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while assigning the role", details = ex.Message });
            }
        }


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

        [HttpPost("{userId}/deactivate")]
        public async Task<IActionResult> Deactivate(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            user.IsActive = false;
            await _userManager.UpdateAsync(user);

         
            var tokens = await _db.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ToListAsync();

            foreach (var t in tokens)
            {
                t.IsRevoked = true;
                t.RevokedAt = DateTime.UtcNow;
                t.ReplacedByToken = "Deactivated by admin";
            }

            await _db.SaveChangesAsync();

            await _audit.LogAsync(userId, "DEACTIVATE_USER", "Users", 0,
                "User deactivated + sessions revoked", GetIp(), GetUA());

            return Ok(new { message = "User deactivated + sessions revoked" });
        }

      
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
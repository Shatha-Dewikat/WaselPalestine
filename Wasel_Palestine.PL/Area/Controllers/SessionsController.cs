using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Utils;

namespace Wasel_Palestine.PL.Controllers
{
    [ApiController]
    [Route("api/auth/sessions")]
    [Authorize]
    public class SessionsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly AuditLogger _audit;

        public SessionsController(ApplicationDbContext db, AuditLogger audit)
        {
            _db = db;
            _audit = audit;
        }

        // POST /api/auth/sessions/logout-all
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var tokens = await _db.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ToListAsync();

            foreach (var t in tokens)
            {
                t.IsRevoked = true;
                t.RevokedAt = DateTime.UtcNow;
                t.ReplacedByToken = "LogoutAll";
            }

            await _db.SaveChangesAsync();
            await _audit.LogAsync(userId, "LOGOUT_ALL", "RefreshToken", 0, "User logged out from all devices", GetIp(), GetUA());

            return Ok(new { message = "Logged out from all devices" });
        }

        // GET /api/auth/sessions
        [HttpGet]
        public async Task<IActionResult> GetActiveSessions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var sessions = await _db.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.Id,
                    t.DeviceInfo,
                    t.IPAddress,
                    t.CreatedAt,
                    t.ExpiresAt
                })
                .ToListAsync();

            return Ok(sessions);
        }

        // DELETE /api/auth/sessions/{refreshTokenId}
        [HttpDelete("{refreshTokenId:int}")]
        public async Task<IActionResult> RevokeSession(int refreshTokenId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Id == refreshTokenId && t.UserId == userId);
            if (token == null) return NotFound("Session not found");

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.ReplacedByToken = "Revoked by user";

            await _db.SaveChangesAsync();
            await _audit.LogAsync(userId, "REVOKE_SESSION", "RefreshToken", refreshTokenId, "User revoked one session", GetIp(), GetUA());

            return Ok(new { message = "Session revoked" });
        }

        private string GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        private string GetUA() => Request.Headers.UserAgent.ToString();
    }
}
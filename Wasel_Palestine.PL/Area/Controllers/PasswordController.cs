using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Utils;

namespace Wasel_Palestine.PL.Controllers
{
    [ApiController]
    [Route("api/auth/password")]
    public class PasswordController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly AuditLogger _audit;
        private readonly IConfiguration _config;
        private readonly EmailService _email;

        public PasswordController(
            UserManager<User> userManager,
            ApplicationDbContext db,
            AuditLogger audit,
            IConfiguration config,
            EmailService email)
        {
            _userManager = userManager;
            _db = db;
            _audit = audit;
            _config = config;
            _email = email;
        }

        // POST /api/auth/password/forgot
        [HttpPost("forgot")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest req)
        {
            var user = await _userManager.FindByEmailAsync(req.Email);

            // لا نكشف إذا الإيميل موجود أو لا
            if (user == null)
                return Ok(new { message = "If the email exists, a reset link was sent." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // 🔹 إذا عندك Frontend حقيقي استخدميه، غير هيك خليها API Page (بنضيفها لاحقًا)
            var resetUrlBase = _config["Frontend:ResetPasswordUrl"] ?? "http://localhost:3000/reset-password";
            var resetUrl = $"{resetUrlBase}?email={Uri.EscapeDataString(req.Email)}&token={Uri.EscapeDataString(token)}";

            try
            {
                await _email.SendAsync(req.Email, "Reset your password",
                    $"<p>Click to reset your password:</p><a href='{resetUrl}'>Reset Password</a>");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to send email: {ex.Message}");
            }

            await _audit.LogAsync(user.Id, "FORGOT_PASSWORD", "User", 0, "Password reset requested", GetIp(), GetUA());
            return Ok(new { message = "If the email exists, a reset link was sent." });
        }

        // POST /api/auth/password/reset
        [HttpPost("reset")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest req)
        {
            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user == null) return BadRequest("Invalid request");

            var result = await _userManager.ResetPasswordAsync(user, req.Token, req.NewPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);

            // revoke كل refresh tokens (logout all devices)
            var tokens = await _db.RefreshTokens.Where(t => t.UserId == user.Id && !t.IsRevoked).ToListAsync();
            foreach (var t in tokens)
            {
                t.IsRevoked = true;
                t.RevokedAt = DateTime.UtcNow;
                t.ReplacedByToken = "Password reset";
            }
            await _db.SaveChangesAsync();

            await _audit.LogAsync(user.Id, "RESET_PASSWORD", "User", 0, "Password reset successful", GetIp(), GetUA());
            return Ok(new { message = "Password updated" });
        }

        // POST /api/auth/password/change
        [HttpPost("change")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest req)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var res = await _userManager.ChangePasswordAsync(user, req.CurrentPassword, req.NewPassword);
            if (!res.Succeeded) return BadRequest(res.Errors);

            // revoke all refresh tokens (logout all devices)
            var tokens = await _db.RefreshTokens.Where(t => t.UserId == user.Id && !t.IsRevoked).ToListAsync();
            foreach (var t in tokens)
            {
                t.IsRevoked = true;
                t.RevokedAt = DateTime.UtcNow;
                t.ReplacedByToken = "Password changed";
            }
            await _db.SaveChangesAsync();

            await _audit.LogAsync(user.Id, "CHANGE_PASSWORD", "User", 0, "Password changed + sessions revoked", GetIp(), GetUA());
            return Ok(new { message = "Password changed" });
        }

        private string GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        private string GetUA() => Request.Headers.UserAgent.ToString();
    }
}
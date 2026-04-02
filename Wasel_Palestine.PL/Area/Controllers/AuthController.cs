using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Utils;
 using Microsoft.AspNetCore.RateLimiting;

namespace Wasel_Palestine.PL.Area.Controllers
{
   

[EnableRateLimiting("auth")]
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _db;
        private readonly TokenService _tokenService;
        private readonly AuditLogger _audit;
        private readonly IConfiguration _config;
        private readonly EmailService _email;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ApplicationDbContext db,
            TokenService tokenService,
            AuditLogger audit,
            IConfiguration config,
            EmailService email)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _tokenService = tokenService;
            _audit = audit;
            _config = config;
            _email = email;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterRequest req)
        {
            var existing = await _userManager.FindByEmailAsync(req.Email);
            if (existing != null) return BadRequest("Email already exists");

            var user = new User
            {
                FullName = req.FullName,
                Email = req.Email,
                UserName = req.Email,
                IsActive = true,
                EmailVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, req.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "User");
            await _audit.LogAsync(user.Id, "REGISTER", "User", 0, "User registered", GetIp(), GetUA());

            // Email confirmation
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmBase = _config["Frontend:ConfirmEmailUrl"] ?? "http://localhost:5034/api/auth/confirm-email";
            var confirmUrl = $"{confirmBase}?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(emailToken)}";

            await _email.SendAsync(user.Email!, "Confirm your email",
                $"<p>Please confirm your email:</p><a href='{confirmUrl}'>Confirm Email</a>");

            await _audit.LogAsync(user.Id, "CONFIRM_EMAIL_SENT", "User", 0, "Confirmation email sent", GetIp(), GetUA());

            return Ok(new { message = "Registered. Please check your email to confirm." });
        }

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return BadRequest("Invalid user");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded) return BadRequest(result.Errors);

            await _audit.LogAsync(user.Id, "CONFIRM_EMAIL", "User", 0, "Email confirmed", GetIp(), GetUA());
            return Ok(new { message = "Email confirmed successfully" });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest req)
        {
            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user == null) return Unauthorized("Invalid credentials");
            if (!user.IsActive) return Unauthorized("User is deactivated");
            if (!user.EmailConfirmed) return Unauthorized("Email not confirmed");

            var signIn = await _signInManager.PasswordSignInAsync(user, req.Password, false, lockoutOnFailure: true);
            if (!signIn.Succeeded) return Unauthorized("Invalid credentials");

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.CreateAccessToken(user, roles);

            var refresh = _tokenService.GenerateRefreshToken();
            var refreshDays = int.TryParse(_config["Jwt:RefreshTokenDays"], out var d) ? d : 14;

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = refresh,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),
                IsRevoked = false,
                ReplacedByToken = "",
                DeviceInfo = req.DeviceInfo ?? "unknown",
                IPAddress = GetIp(),
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            await _audit.LogAsync(user.Id, "LOGIN", "User", 0, "User logged in", GetIp(), GetUA());

            return Ok(new AuthResponse { AccessToken = accessToken, RefreshToken = refresh });
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(RefreshRequest req)
        {
            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == req.RefreshToken);
            if (stored == null) return Unauthorized("Invalid refresh token");

            // reuse detection
            if (stored.IsRevoked)
            {
                await RevokeAllUserTokens(stored.UserId, "Refresh token reuse detected");
                return Unauthorized("Refresh token reuse detected. All sessions revoked.");
            }

            if (stored.ExpiresAt < DateTime.UtcNow) return Unauthorized("Refresh token expired");

            var user = await _userManager.FindByIdAsync(stored.UserId);
            if (user == null || !user.IsActive) return Unauthorized("User invalid/deactivated");

            var roles = await _userManager.GetRolesAsync(user);
            var newAccess = _tokenService.CreateAccessToken(user, roles);

            // rotation
            var newRefresh = _tokenService.GenerateRefreshToken();
            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;
            stored.ReplacedByToken = newRefresh;

            var refreshDays = int.TryParse(_config["Jwt:RefreshTokenDays"], out var d) ? d : 14;

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = newRefresh,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),
                IsRevoked = false,
                ReplacedByToken = "",
                DeviceInfo = req.DeviceInfo ?? stored.DeviceInfo,
                IPAddress = GetIp(),
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            await _audit.LogAsync(user.Id, "REFRESH", "RefreshToken", 0, "Token refreshed", GetIp(), GetUA());

            return Ok(new AuthResponse { AccessToken = newAccess, RefreshToken = newRefresh });
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout(RefreshRequest req)
        {
            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == req.RefreshToken);
            if (stored == null) return Ok(new { message = "Logged out" });

            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _audit.LogAsync(stored.UserId, "LOGOUT", "RefreshToken", 0, "Logged out (single session)", GetIp(), GetUA());
            return Ok(new { message = "Logged out" });
        }

        private async Task RevokeAllUserTokens(string userId, string reason)
        {
            var tokens = await _db.RefreshTokens.Where(t => t.UserId == userId && !t.IsRevoked).ToListAsync();
            foreach (var t in tokens)
            {
                t.IsRevoked = true;
                t.RevokedAt = DateTime.UtcNow;
                t.ReplacedByToken = reason;
            }

            await _db.SaveChangesAsync();
            await _audit.LogAsync(userId, "REVOKE_ALL", "RefreshToken", 0, reason, GetIp(), GetUA());
        }
         
         [HttpPost("resend-confirmation")]
[AllowAnonymous]
public async Task<IActionResult> ResendConfirmation(ForgotPasswordRequest req)
{
    var user = await _userManager.FindByEmailAsync(req.Email);

    // لا نكشف إذا الإيميل موجود أو لا
    if (user == null)
        return Ok(new { message = "If the email exists, a confirmation link was sent." });

    if (user.EmailConfirmed)
        return Ok(new { message = "Email already confirmed." });

    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

    var confirmBase = _config["Frontend:ConfirmEmailUrl"] ?? "http://localhost:5034/api/auth/confirm-email";
    var confirmUrl =
        $"{confirmBase}?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}";

    await _email.SendAsync(user.Email!, "Confirm your email (Resent)",
        $"<p>Click to confirm your email:</p><a href='{confirmUrl}'>Confirm Email</a>");

    await _audit.LogAsync(user.Id, "RESEND_CONFIRM_EMAIL", "User", 0, "Confirmation email resent", GetIp(), GetUA());

    return Ok(new { message = "If the email exists, a confirmation link was sent." });
}
        private string GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        private string GetUA() => Request.Headers.UserAgent.ToString();
    }
}
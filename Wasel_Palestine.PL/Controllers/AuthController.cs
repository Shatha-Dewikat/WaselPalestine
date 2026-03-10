using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Utils;
using Wasel_Palestine.PL.DTO.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Text.Encodings.Web;


using System.Security.Claims;

namespace Wasel_Palestine.PL.Controllers
{
    
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous]
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

            // password hashing automatic by Identity
            var result = await _userManager.CreateAsync(user, req.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            // default role
            await _userManager.AddToRoleAsync(user, "User");

            await _audit.LogAsync(user.Id, "REGISTER", "User", 0, "User registered", GetIp(), GetUA());

var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

var confirmBase = _config["Frontend:ConfirmEmailUrl"] ?? "http://localhost:5034/api/auth/confirm-email";
var confirmUrl =
    $"{confirmBase}?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(emailToken)}";

await _email.SendAsync(user.Email!, "Confirm your email",
    $"<p>Please confirm your email:</p><a href='{confirmUrl}'>Confirm Email</a>");

await _audit.LogAsync(user.Id, "CONFIRM_EMAIL_SENT", "User", 0, "Confirmation email sent", GetIp(), GetUA());

           return Ok(new { message = "Registered. Please check your email to confirm." });

            
        }


        [HttpGet("confirm-email")]
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
        public async Task<IActionResult> Login(LoginRequest req)
        {
            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user == null) return Unauthorized("Invalid credentials");
            if (!user.IsActive) return Unauthorized("User is deactivated");
            if (!user.EmailConfirmed)
    return Unauthorized("Email not confirmed");

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
        

        private string GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        private string GetUA() => Request.Headers.UserAgent.ToString();

        [HttpPost("logout-all")]
             [Authorize]
public async Task<IActionResult> LogoutAll()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    if (string.IsNullOrWhiteSpace(userId))
        return Unauthorized();

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

[HttpGet("sessions")]
[Authorize]
public async Task<IActionResult> GetActiveSessions()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    if (string.IsNullOrWhiteSpace(userId))
        return Unauthorized();

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
[HttpDelete("sessions/{refreshTokenId:int}")]
[Authorize]
public async Task<IActionResult> RevokeSession(int refreshTokenId)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    if (string.IsNullOrWhiteSpace(userId))
        return Unauthorized();

    var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Id == refreshTokenId && t.UserId == userId);
    if (token == null) return NotFound("Session not found");

    token.IsRevoked = true;
    token.RevokedAt = DateTime.UtcNow;
    token.ReplacedByToken = "Revoked by user";

    await _db.SaveChangesAsync();
    await _audit.LogAsync(userId, "REVOKE_SESSION", "RefreshToken", refreshTokenId, "User revoked one session", GetIp(), GetUA());

    return Ok(new { message = "Session revoked" });
}
[HttpPost("forgot-password")]
public async Task<IActionResult> ForgotPassword(Wasel_Palestine.PL.DTO.Auth.ForgotPasswordRequest req)
{
    var user = await _userManager.FindByEmailAsync(req.Email);

    // ما نكشف إذا الإيميل موجود أو لا
    if (user == null)
        return Ok(new { message = "If the email exists, a reset link was sent." });

    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

    var resetUrlBase = _config["Frontend:ResetPasswordUrl"] ?? "http://localhost:3000/reset-password";
    var resetUrl =
        $"{resetUrlBase}?email={Uri.EscapeDataString(req.Email)}&token={Uri.EscapeDataString(token)}";

    // إذا SMTP مش متجهز، رجّع token للتجربة فقط (Development)
    try
    {
        await _email.SendAsync(req.Email, "Reset your password",
            $"<p>Click to reset your password:</p><a href='{resetUrl}'>Reset Password</a>");
    }
    catch
    {
        if (HttpContext.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() == true)
        {
            return Ok(new
            {
                message = "DEV MODE: Email not configured. Use token below.",
                email = req.Email,
                token = token
            });
        }

        throw;
    }

    await _audit.LogAsync(user.Id, "FORGOT_PASSWORD", "User", 0, "Password reset requested", GetIp(), GetUA());
    return Ok(new { message = "If the email exists, a reset link was sent." });
}

[HttpPost("reset-password")]
public async Task<IActionResult> ResetPassword(Wasel_Palestine.PL.DTO.Auth.ResetPasswordRequest req)
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


    }
}
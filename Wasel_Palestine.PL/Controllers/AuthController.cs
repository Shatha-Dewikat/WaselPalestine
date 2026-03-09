using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Utils;
using Wasel_Palestine.PL.DTO.Auth;
using Microsoft.AspNetCore.Authorization;

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

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ApplicationDbContext db,
            TokenService tokenService,
            AuditLogger audit,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _tokenService = tokenService;
            _audit = audit;
            _config = config;
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

            return Ok(new { message = "Registered" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest req)
        {
            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user == null) return Unauthorized("Invalid credentials");
            if (!user.IsActive) return Unauthorized("User is deactivated");

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
    }
}
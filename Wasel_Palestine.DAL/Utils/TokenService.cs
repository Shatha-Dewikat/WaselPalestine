using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Utils
{
    public class TokenService
    {
        private readonly IConfiguration _config;
        public TokenService(IConfiguration config) => _config = config;

        public string CreateAccessToken(User user, IList<string> roles)
        {
            var jwt = _config.GetSection("Jwt");

            var keyStr = jwt["Key"];
            if (string.IsNullOrWhiteSpace(keyStr))
                throw new InvalidOperationException("JWT Key is missing. Add Jwt:Key in appsettings.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim("isActive", user.IsActive ? "true" : "false"),
                new Claim("fullName", user.FullName ?? "")
            };

            foreach (var r in roles)
                claims.Add(new Claim(ClaimTypes.Role, r));

            var issuer = jwt["Issuer"] ?? "WaselPalestine";
            var audience = jwt["Audience"] ?? "WaselPalestine";

            var minutes = 15;
            int.TryParse(jwt["AccessTokenMinutes"], out minutes);
            if (minutes <= 0) minutes = 15;

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            // 64 bytes => strong token
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }
    }
}
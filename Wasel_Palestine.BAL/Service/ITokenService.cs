using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.BLL.Service
{
    public interface ITokenService
    {
        Task<string> GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

    }
}

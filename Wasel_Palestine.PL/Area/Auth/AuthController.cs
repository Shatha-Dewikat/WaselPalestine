using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Wasel_Palestine.BAL.Service;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Utils;


namespace Wasel_Palestine.PL.Area.Auth
{

    [ApiController]
    [Route("api/auth/[controller]")]
    [AllowAnonymous]
    public class AuthController(IAuthenticationService authenticationService) : ControllerBase
    {
        [HttpPost("Register")]
        public async Task<IActionResult> Register(DAL.DTO.Request.RegisterRequest request)
        {

            var result = await authenticationService.RegisterAsync(request);
            return Ok(result);
        }




        [HttpPost("Login")]
        public async Task<IActionResult> Login(DAL.DTO.Request.LoginRequest request)
        {
            var result = await authenticationService.LoginAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPatch("RefreshToken")]
        public async Task<IActionResult> RefreshToken(TokenApiModel request)
        {
            var result = await authenticationService.RefreshTokenAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string userId)
        {
            var result = await authenticationService.ConfirmEmailAsync(token, userId);

            if (!result)
                return BadRequest("Email confirmation failed");

            return Ok("Email confirmed successfully");
        }
        [HttpPost("SendCode")]
        public async Task<IActionResult> RequestPasswordReset(DAL.DTO.Request.ForgotPasswordRequest request)
        {
            var result = await authenticationService.RequestPasswordReset(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPatch("ResetPassword")]
        public async Task<IActionResult> ResetPassword(DAL.DTO.Request.ResetPasswordRequest request)
        {
            var result = await authenticationService.ResetPassword(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

    }
}
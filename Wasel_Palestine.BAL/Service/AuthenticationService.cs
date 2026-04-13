using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.BLL.Service
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;

        public AuthenticationService(UserManager<User> userManager, IConfiguration configuration, ITokenService tokenService, SignInManager<User> signInManager, IEmailSender emailSender
            )
        {
            _userManager = userManager;
            _configuration = configuration;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginRequest.Email);

                if (user is null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "invalid email"
                    };
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, lockoutOnFailure: true);

                if (result.IsLockedOut)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Your account is locked due to too many incorrect attempts. Please wait 10 minutes."
                    };
                }
                if (!result.Succeeded)
                {
                    return new LoginResponse { Success = false, Message = "invalid password" };
                }
               
                if (!user.IsActive)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Your account has been deactivated. Please contact support."
                    };
                }
                if (!user.EmailConfirmed)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Please confirm your email first"
                    };
                }
                var accessToken = await _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

                await _userManager.UpdateAsync(user);

                return new LoginResponse()
                {
                    Success = true,
                    Message = "Login successfully",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };

            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "An unexpected error",
                    Errors = new List<string> { ex.Message }
                };
            }
        }




        // REGISTER
        public async Task<RegisterResponse> RegisterAsync(RegisterRequest registerRequest)
        {
            try
            {
                // تحقق من وجود الايميل مسبقاً
                var existingUser = await _userManager.FindByEmailAsync(registerRequest.Email);
                if (existingUser != null)
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "Email already exists"
                    };
                }

                var user = registerRequest.Adapt<User>();
                var result = await _userManager.CreateAsync(user, registerRequest.Password);

                if (!result.Succeeded)
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "User Creation Failed",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                await _userManager.AddToRoleAsync(user, "User");


                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

              
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                var confirmationLink = $"http://localhost:32768/api/auth/Auth/ConfirmEmail?userId={user.Id}&token={encodedToken}";
                //                var confirmationLink = $"http://localhost:5034/api/auth/Auth/ConfirmEmail?userId={user.Id}&token={encodedToken}";

                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Confirm your email",
                    $"<h3>Welcome {user.UserName}!</h3>" +
                    $"<p>Please confirm your email by clicking the link below:</p>" +
                    $"<a href='{confirmationLink}'>Confirm Email</a>"
                );
                return new RegisterResponse
                {
                    Success = true,
                    Message = "Success"
                };
            }
            catch (Exception ex)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "An unexpected error",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<LoginResponse> RefreshTokenAsync(TokenApiModel request)
        {
            if (request.AccessToken is null || request.RefreshToken is null)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid client request"
                };
            }

            var accessToken = request.AccessToken;
            var refreshToken = request.RefreshToken;

            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            var userName = principal.Identity?.Name;

            if (string.IsNullOrEmpty(userName))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid token"
                };
            }

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null ||
                user.RefreshToken != refreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid client request"
                };
            }

            var newAccessToken = await _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            return new LoginResponse
            {
                Success = true,
                Message = "Token refreshed successfully",
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }



        public async Task<ForgotPasswordResponse> RequestPasswordReset(ForgotPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                return new ForgotPasswordResponse
                {
                    Success = false,
                    Message = "Email Not Found"
                };
            }

            var random = new Random();
            var code = random.Next(1000, 9999).ToString();

            user.CodeResetPassword = code;
            user.PasswordResetCodeExpiry = DateTime.UtcNow.AddMinutes(15);

            await _userManager.UpdateAsync(user);

            await _emailSender.SendEmailAsync(
                request.Email,
                "reset password",
                $"<p>code is {code}</p>"
            );

            return new ForgotPasswordResponse
            {
                Success = true,
                Message = "Code sent to your email"
            };


        }

        public async Task<bool> ConfirmEmailAsync(string token, string userId)
        {


            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            user.IsActive = true;    
            user.EmailVerified = true; 
            await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
        public async Task<ResetPasswordResponse> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Email Not Found"
                };

            if (user.CodeResetPassword != request.Code)
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Invalid code"
                };

            if (user.PasswordResetCodeExpiry < DateTime.UtcNow)
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Code expired"
                };
         
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!result.Succeeded)
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Password reset failed",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            await _emailSender.SendEmailAsync(
                request.Email,
                "Password Changed",
                "<p>Your password has been changed successfully.</p>"
            );

            return new ResetPasswordResponse
            {
                Success = true,
                Message = "Password reset successfully"
            };
        }

    }

}

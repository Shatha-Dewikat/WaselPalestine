namespace Wasel_Palestine.PL.DTO.Auth
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; } = "";
        public string Token { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }
}

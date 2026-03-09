namespace Wasel_Palestine.PL.DTO.Auth
{
    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string? DeviceInfo { get; set; }
    }
}
namespace Wasel_Palestine.DAL.DTO.Request
{
    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        
        public string? DeviceInfo { get; set; }
    }
}
namespace Wasel_Palestine.PL.DTO.Auth
{
    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = "";
        public string? DeviceInfo { get; set; }
    }
}
namespace Wasel_Palestine.DAL.DTO.Request
{
    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = "";
        public string? DeviceInfo { get; set; }
        
    }
}
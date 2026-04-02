namespace Wasel_Palestine.DAL.DTO.Request
{
    public class AdminResetPasswordRequest
    {
        // if empty --> default pass
        public string? TempPassword { get; set; }
    }
}
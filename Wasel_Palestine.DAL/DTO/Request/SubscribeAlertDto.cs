 namespace Wasel_Palestine.DAL.DTO.Request
{
    public class SubscribeAlertDto
    {
        public string? UserId { get; set; }
        public int LocationId { get; set; }
        public int CategoryId { get; set; }
    }
}
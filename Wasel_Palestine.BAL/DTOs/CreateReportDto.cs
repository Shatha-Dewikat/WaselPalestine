namespace Wasel_Palestine.BAL.DTOs
{
    public class CreateReportDto
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; } // مثل: حاجز، أزمة، إغلاق
        public string? UserId { get; set; } // لربط البلاغ بصاحبه (شغل Person 1)
    }
}
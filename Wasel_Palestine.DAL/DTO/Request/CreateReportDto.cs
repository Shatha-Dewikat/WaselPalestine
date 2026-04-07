namespace Wasel_Palestine.BAL.DTOs
{
    public class CreateReportDto
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? UserId { get; set; } 
        public string? City { get; set; }
        public string? AreaName { get; set; }
    }
}
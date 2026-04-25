namespace Wasel_Palestine.BAL.DTOs
{
    public class ActiveReportDto
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CategoryName { get; set; }
        public float  ConfidenceScore { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
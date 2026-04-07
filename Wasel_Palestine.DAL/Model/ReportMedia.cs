using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class ReportMedia
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public Report Report { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public string? MediaUrl { get; set; } 
        public string? MediaType { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class Report
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public int CategoryId { get; set; }
        public IncidentCategory Category { get; set; }

        public int StatusId { get; set; }
        public ReportStatus Status { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; }

        public string Description { get; set; }
        public float ConfidenceScore { get; set; }
        public int? DuplicateOfReportId { get; set; }
        public Report DuplicateOfReport { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public List<ReportVote> ReportVotes { get; set; }
        public List<ReportMedia> ReportMedias { get; set; }
        public List<ReportModerationAction> ReportModerationActions { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class Incident
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string TitleAr { get; set; }
        public string DescriptionAr { get; set; }
        public int CategoryId { get; set; }
        public IncidentCategory Category { get; set; }

        public int SeverityId { get; set; }
        public IncidentSeverity Severity { get; set; }

        public int StatusId { get; set; }
        public IncidentStatus Status { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; }

        public int? CheckpointId { get; set; }
        public Checkpoint Checkpoint { get; set; }

        public string? CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }

        public bool Verified { get; set; }
        public string? VerifiedByUserId { get; set; }
        public User VerifiedByUser { get; set; }
        public DateTime? VerifiedAt { get; set; }

        public string? ClosedByUserId { get; set; }
        public User ClosedByUser { get; set; }
        public DateTime? ClosedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public List<IncidentHistory> IncidentHistories { get; set; }
        public List<IncidentMedia> IncidentMedia { get; set; }
        public List<Alert> Alerts { get; set; } = new List<Alert>();
         public string TitleAr { get; set; }
 public string DescriptionAr { get; set; }

    }
}

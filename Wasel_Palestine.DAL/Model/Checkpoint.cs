using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class Checkpoint
    {
        public int Id { get; set; }

        public string NameEn { get; set; }
        public string NameAr { get; set; }

        public string DescriptionEn { get; set; }
        public string DescriptionAr { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; }

        public string CurrentStatus { get; set; }

        public int? EstimatedDelayMinutes { get; set; }

        public double ConfidenceScore { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public List<CheckpointStatusHistory> StatusHistories { get; set; } = new List<CheckpointStatusHistory>();
        public List<Incident> Incidents { get; set; } = new List<Incident>();
    }
}

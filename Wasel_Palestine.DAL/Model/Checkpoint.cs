using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class Checkpoint
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; }

        public string CurrentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public List<CheckpointStatusHistory> StatusHistories { get; set; }
        public List<Incident> Incidents { get; set; }
    }
}

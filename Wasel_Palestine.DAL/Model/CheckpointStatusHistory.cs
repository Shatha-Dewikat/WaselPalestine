using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class CheckpointStatusHistory
    {
        public int Id { get; set; }

        public int CheckpointId { get; set; }
        public Checkpoint Checkpoint { get; set; }

        public string Status { get; set; }
        public DateTime ChangedAt { get; set; }

        public string ChangedByUserId { get; set; }
        public User ChangedByUser { get; set; }
    }
}

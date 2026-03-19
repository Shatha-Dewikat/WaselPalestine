using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class CheckpointStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public bool IsActive { get; set; } = true;

        public List<Checkpoint> Checkpoints { get; set; }
    }
}

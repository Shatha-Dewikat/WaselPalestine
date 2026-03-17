using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.DTO.Response
{
    public class CheckpointResponse
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public int? EstimatedDelayMinutes { get; set; }

        public double ConfidenceScore { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

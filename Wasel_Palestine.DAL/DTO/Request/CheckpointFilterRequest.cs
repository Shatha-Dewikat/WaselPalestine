using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.DTO.Request
{
    public class CheckpointFilterRequest
    {
        public string ?Status { get; set; }
        public int? LocationId { get; set; }
        public string? City { get; set; }
    }
}

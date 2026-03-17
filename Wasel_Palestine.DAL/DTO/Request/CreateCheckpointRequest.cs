using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.DTO.Request
{
    public class CreateCheckpointRequest
    {
        public string NameEn { get; set; }
        public string NameAr { get; set; }

        public string DescriptionEn { get; set; }
        public string DescriptionAr { get; set; }

        public int LocationId { get; set; }

        public string Status { get; set; }

        public int? EstimatedDelayMinutes { get; set; }
    }
}

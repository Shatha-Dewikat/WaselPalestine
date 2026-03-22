using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.DTO.Response
{
    public class IncidentResponse : BaseResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Severity { get; set; }
        public string Status { get; set; }
        public bool Verified { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<IncidentMediaResponse> Media { get; set; } = new();
        public int? RelatedCheckpointId { get; set; }

        public List<IncidentHistoryResponse> History { get; set; } = new();
        
    }
}

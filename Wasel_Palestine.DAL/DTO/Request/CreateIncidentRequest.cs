using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.DTO.Request
{
    public class CreateIncidentRequest
    {
        public string Title { get; set; }
        public string TitleAr { get; set; }        
        public string Description { get; set; }
        public string DescriptionAr { get; set; }

        public int CategoryId { get; set; }

        public int SeverityId { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int? CheckpointId { get; set; }

    }
}

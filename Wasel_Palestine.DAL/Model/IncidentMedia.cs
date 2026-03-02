using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class IncidentMedia
    {
        public int Id { get; set; }
        public int IncidentId { get; set; }
        public Incident Incident { get; set; }

        public string Url { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

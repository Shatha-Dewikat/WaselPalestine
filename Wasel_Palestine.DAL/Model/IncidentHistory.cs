using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class IncidentHistory
    {
        public int Id { get; set; }

        public int IncidentId { get; set; }
        public Incident Incident { get; set; }

        public int StatusId { get; set; }
        public IncidentStatus Status { get; set; }

        public DateTime ChangedAt { get; set; }
        public string Action { get; set; }
        public String ChangedByUserId { get; set; }
        public User ChangedByUser { get; set; }

        public string Changes { get; set; }
    }
}

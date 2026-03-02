using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class IncidentStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Incident> Incidents { get; set; }
        public List<IncidentHistory> IncidentHistories { get; set; }
    }
}

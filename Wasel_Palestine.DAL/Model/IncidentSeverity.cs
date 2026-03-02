using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class IncidentSeverity
    {

        public int Id { get; set; }
        public string Name { get; set; }

        public List<Incident> Incidents { get; set; }
    }
}

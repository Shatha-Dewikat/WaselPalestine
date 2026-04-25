using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class CityIncidentStats
    {
        public int Id { get; set; }
        public string City { get; set; }
        public int ActiveIncidentsCount { get; set; }
        public int ClosedCheckpointsCount { get; set; }
    }
}

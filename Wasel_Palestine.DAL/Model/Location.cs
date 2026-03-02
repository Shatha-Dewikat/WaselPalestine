using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class Location
    {
        public int Id { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string AreaName { get; set; }
        public string City { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<RouteRequest> FromRouteRequests { get; set; } = new List<RouteRequest>();
        public List<RouteRequest> ToRouteRequests { get; set; } = new List<RouteRequest>();

        public List<Incident> Incidents { get; set; } = new List<Incident>();
        public List<Checkpoint> Checkpoints { get; set; } = new List<Checkpoint>();
    }
}

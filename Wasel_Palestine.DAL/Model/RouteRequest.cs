using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class RouteRequest
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public User User { get; set; }

        public int FromLocationId { get; set; }
        public Location FromLocation { get; set; }

        public int ToLocationId { get; set; }
        public Location ToLocation { get; set; }

        public float EstimatedDistance { get; set; }
        public float EstimatedDuration { get; set; }
        public string RouteGeometry { get; set; }
        public bool AvoidCheckpoints { get; set; }
        public string RouteType { get; set; }
        public string Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

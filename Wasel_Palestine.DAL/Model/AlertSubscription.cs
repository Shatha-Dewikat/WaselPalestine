using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class AlertSubscription
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; }

        public int CategoryId { get; set; }
        public IncidentCategory Category { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

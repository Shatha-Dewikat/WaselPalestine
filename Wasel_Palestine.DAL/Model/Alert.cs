using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class Alert
    {
        public int Id { get; set; }
        public int IncidentId { get; set; }
        public Incident Incident { get; set; }
        public DateTime CreatedAt { get; set; }=DateTime.UtcNow;

        public List<AlertRecipient> Recipients { get; set; } = new List<AlertRecipient>();
        public List<AlertHistory> AlertHistories { get; set; } = new List<AlertHistory>();
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class AlertHistory
    {
        public int Id { get; set; }
        public int AlertId { get; set; }
        public Alert Alert { get; set; }

        public string Status { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

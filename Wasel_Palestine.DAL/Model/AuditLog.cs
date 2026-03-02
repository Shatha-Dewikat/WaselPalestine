using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public string Action { get; set; }
        public string EntityName { get; set; }
        public int EntityId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
    }
}

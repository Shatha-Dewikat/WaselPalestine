using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class AlertRecipient
    {
        public int Id { get; set; }
        public int AlertId { get; set; }
        public Alert Alert { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

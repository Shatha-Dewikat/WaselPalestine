using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class UserSession
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }

        public string DeviceInfo { get; set; }
        public string IPAddress { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActivityAt { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class ExternalApiCache
    {
        public int Id { get; set; }
        public string Provider { get; set; }
        public string RequestHash { get; set; }
        public string Response { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

    }
}

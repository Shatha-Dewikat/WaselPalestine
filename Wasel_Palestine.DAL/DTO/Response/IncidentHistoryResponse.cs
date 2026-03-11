using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.DTO.Response
{
    public class IncidentHistoryResponse
    {
        public int StatusId { get; set; }
        public string Status { get; set; }
        public string ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; }
        public string Action { get; set; }
        public string Changes { get; set; }
    }
}

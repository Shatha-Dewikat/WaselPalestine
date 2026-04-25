using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.DTO.Response
{
    public class AlertResponse : BaseResponse
    {
        public int Id { get; set; }
        public int IncidentId { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}

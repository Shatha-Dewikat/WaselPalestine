using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.DTO.Request
{
    public class IncidentFilterRequest
    {
        public int? CategoryId { get; set; }
        public int? SeverityId { get; set; }
        public int? StatusId { get; set; }

        public Location? LocationName { get; set; }

    }
}

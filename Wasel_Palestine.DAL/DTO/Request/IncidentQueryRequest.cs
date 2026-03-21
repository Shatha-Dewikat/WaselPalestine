using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.DTO.Request
{
    public class IncidentQueryRequest
    {

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public int? StatusId { get; set; }
        public int? CategoryId { get; set; }
        public int? SeverityId { get; set; }

       
        public int? RelatedCheckpointId { get; set; }

        public IncidentFilterRequest Filter { get; set; }
        public PaginationRequest Pagination { get; set; }
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    
}
}

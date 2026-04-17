using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Wasel_Palestine.DAL.DTO.Request
{
    public class IncidentMediaCreateRequest
    {
        public int IncidentId { get; set; }

        public IFormFile File { get; set; }
    }
}

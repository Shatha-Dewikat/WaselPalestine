using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Wasel_Palestine.DAL.DTO.Request
{
    public class IncidentSeverityCreateRequest
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public int Level { get; set; }
    }
}

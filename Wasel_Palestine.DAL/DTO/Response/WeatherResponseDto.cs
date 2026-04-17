using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.DTO.Response
{
    public class WeatherResponseDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Temperature { get; set; }
        public string? Condition { get; set; } 
        public string? Description { get; set; } 
        public DateTime LastUpdated { get; set; }
    }
}

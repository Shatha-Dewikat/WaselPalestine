using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.DTO.Response
{
    public class HeatmapPointResponse
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int Intensity { get; set; }
    }
}
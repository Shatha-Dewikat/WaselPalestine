using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.BLL.Helper
{
    public static class PalestineGridHelper
    {
        public static List<(double Lat, double Lon)> GenerateGrid(double step = 0.4)
        {
            var points = new List<(double, double)>();
            for (double lat = 31.0; lat <= 32.5; lat += step)
            {
                for (double lon = 34.2; lon <= 35.7; lon += step)
                {
                    points.Add((lat, lon));
                }
            }
            return points;
        }
    }
}

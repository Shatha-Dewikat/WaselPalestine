using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.PL.DTOs;

namespace Wasel_Palestine.BAL.Service
{
    public interface IWeatherService
    {
        Task<WeatherResponseDto> GetCurrentWeatherAsync(double lat, double lon);
    }
}

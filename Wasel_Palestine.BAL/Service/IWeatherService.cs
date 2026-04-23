using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Response;

namespace Wasel_Palestine.BAL.Service
{
    public interface IWeatherService
    {
        Task<WeatherResponseDto> GetCurrentWeatherAsync(double lat, double lon);
    }
}

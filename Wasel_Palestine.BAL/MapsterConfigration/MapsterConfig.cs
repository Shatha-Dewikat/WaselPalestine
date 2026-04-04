using Mapster;
using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.BLL.MapsterConfigration
{
    public class MapsterConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<Incident, IncidentResponse>.NewConfig()
                .Map(dest => dest.Category, src => src.Category.Name)
                .Map(dest => dest.Severity, src => src.Severity.Name)
                .Map(dest => dest.Status, src => src.Status.Name)
                .Map(dest => dest.Latitude, src => (double)src.Location.Latitude)
                .Map(dest => dest.Longitude, src => (double)src.Location.Longitude);
        }

    }
}

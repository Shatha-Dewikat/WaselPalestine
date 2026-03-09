using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;

namespace Wasel_Palestine.BLL.Service
{
    public interface IIncidentSeverityService
    {
        Task<IncidentSeverityResponse> CreateIncidentSeverityAsync(
            IncidentSeverityCreateRequest request,
            string userId);

        Task<IncidentSeverityResponse> UpdateIncidentSeverityAsync(
            int id,
            IncidentSeverityUpdateRequest request,
            string userId);

        Task DeleteIncidentSeverityAsync(int id, string userId);

        Task<IncidentSeverityResponse> GetIncidentSeverityByIdAsync(int id);

        Task<List<IncidentSeverityResponse>> GetAllIncidentSeveritiesAsync();
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using System.Threading.Tasks;

namespace Wasel_Palestine.BAL.Service
{
    public interface IIncidentSeverityService
    {
        Task<IncidentSeverityResponse> CreateIncidentSeverityAsync(
            IncidentSeverityCreateRequest request,
            string userId,
            string ip,
            string userAgent);

        Task<IncidentSeverityResponse> UpdateIncidentSeverityAsync(
            int id,
            IncidentSeverityUpdateRequest request,
            string userId,
            string ip,
            string userAgent);

        Task DeleteIncidentSeverityAsync(
            int id,
            string userId,
            string ip,
            string userAgent);

        Task<IncidentSeverityResponse> GetIncidentSeverityByIdAsync(int id);

        Task<List<IncidentSeverityResponse>> GetAllIncidentSeveritiesAsync();
    }
}
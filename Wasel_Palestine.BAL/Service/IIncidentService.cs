using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;

namespace Wasel_Palestine.BLL.Service
{
    public interface IIncidentService
    {
        Task<IncidentResponse> CreateIncidentAsync(CreateIncidentRequest request, string userId);
        Task<IncidentResponse> UpdateIncidentAsync(int id, UpdateIncidentRequest request);
        Task<IncidentResponse> GetIncidentByIdAsync(int id);
        Task<List<IncidentResponse>> GetIncidentAllAsync();

        Task<IncidentResponse> DeleteIncidentAsync(int id);
    }
}

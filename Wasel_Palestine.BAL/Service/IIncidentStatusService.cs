using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;

namespace Wasel_Palestine.BLL.Service
{
    public interface IIncidentStatusService
    {
        Task<IncidentStatusResponse> CreateStatusAsync(IncidentStatusCreateRequest request, string userId, string ip, string userAgent);
        Task<IncidentStatusResponse> UpdateStatusAsync(int id, IncidentStatusUpdateRequest request, string userId, string ip, string userAgent);
        Task DeleteStatusAsync(int id, string userId, string ip, string userAgent);

        Task<IncidentStatusResponse> GetStatusByIdAsync(int id);

        Task<List<IncidentStatusResponse>> GetAllStatusesAsync();
    }
}

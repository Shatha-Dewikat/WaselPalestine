using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;

namespace Wasel_Palestine.BLL.Service
{
    public interface IIncidentStatusService
    {
        Task<IncidentStatusResponse> CreateStatusAsync(
            IncidentStatusCreateRequest request,
            string userId);

        Task<IncidentStatusResponse> UpdateStatusAsync(
            int id,
            IncidentStatusUpdateRequest request,
            string userId);

        Task DeleteStatusAsync(int id, string userId);

        Task<IncidentStatusResponse> GetStatusByIdAsync(int id);

        Task<List<IncidentStatusResponse>> GetAllStatusesAsync();
    }
}

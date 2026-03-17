using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;

namespace Wasel_Palestine.BLL.Service
{
    public interface ICheckpointService
    {
        Task<CheckpointResponse> CreateCheckpointAsync(CreateCheckpointRequest request, string userId, string ip, string userAgent);
        Task<List<CheckpointResponse>> GetAllCheckpointsAsync(string lang);
        Task<List<CheckpointResponse>> GetFilteredCheckpointsAsync(CheckpointFilterRequest filter, string lang);
        Task<List<CheckpointResponse>> GetPagedCheckpointsAsync(CheckPointPaginationRequest pagination, string lang);
        Task<CheckpointResponse> GetCheckpointByIdAsync(int id, string lang);
        Task<bool> UpdateCheckpointAsync(int id, UpdateCheckpointRequest request, string userId, string ip, string userAgent);
        Task<bool> DeleteCheckpointAsync(int id, string userId, string ip, string userAgent);
        Task<bool> ChangeStatusAsync(int id, ChangeCheckpointStatusRequest request, string userId, string ip, string userAgent);
        Task<List<CheckpointHistoryResponse>> GetCheckpointHistoryAsync(int checkpointId);
    }
}

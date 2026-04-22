using System.Collections.Generic;
using System.Threading.Tasks;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;

namespace Wasel_Palestine.BAL.Service
{
    public interface ICheckpointService
    {
        Task<CheckpointResponse> CreateCheckpointAsync(CreateCheckpointRequest request, string userId, string ip, string userAgent);
        Task<CheckpointResponse> UpdateCheckpointAsync(int id, UpdateCheckpointRequest request, string userId, string ip, string userAgent);
        Task<CheckpointResponse> DeleteCheckpointAsync(int id, string userId, string ip, string userAgent);
        Task<CheckpointResponse> RestoreCheckpointAsync(int id, string userId, string ip, string userAgent);
        Task<CheckpointResponse> ChangeStatusAsync(int id, ChangeCheckpointStatusRequest request, string userId, string ip, string userAgent);
        Task<bool> ProcessConfirmedReportAsync(int checkpointId, string newStatus, string userId, string ip, string userAgent);
        Task<List<CheckpointResponse>> GetAllCheckpointsAsync(string lang);
        Task<CheckpointResponse> GetCheckpointByIdAsync(int id, string lang);
        Task<List<CheckpointResponse>> GetFilteredCheckpointsAsync(CheckpointFilterRequest filter, string lang);
        Task<List<CheckpointResponse>> GetPagedCheckpointsAsync(CheckPointPaginationRequest pagination, string lang);
        Task<List<CheckpointHistoryResponse>> GetCheckpointHistoryAsync(int checkpointId);
        Task<List<CheckpointResponse>> GetNearbyCheckpointsAsync(double userLat, double userLon, double radiusInKm, string lang);
    }
    
}
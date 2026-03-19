using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public interface ICheckpointRepository
    {
        Task<Checkpoint> CreateCheckpointAsync(Checkpoint checkpoint);
        Task<List<Checkpoint>> GetAllCheckpointsAsync();

        Task<Checkpoint> GetCheckpointByIdAsync(int id);

        Task<Checkpoint> GetCheckpointEvenDeletedAsync(int id);

        Task UpdateCheckpointAsync(Checkpoint checkpoint);

        Task AddStatusHistoryAsync(CheckpointStatusHistory history);

        Task<List<CheckpointStatusHistory>> GetCheckpointHistoryAsync(int checkpointId);
        Task<Location> GetLocationByIdAsync(int locationId);
        Task<CheckpointStatus> GetStatusByNameAsync(string name);
        Task AddAuditLogAsync(AuditLog log);
        Task<bool> CheckpointStatusExistsAsync(string status);


    }
}

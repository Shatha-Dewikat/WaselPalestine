using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public interface ICheckpointStatusRepository
    {
        Task<List<CheckpointStatus>> GetAllAsync();
        Task<CheckpointStatus> CreateAsync(CheckpointStatus status);
        Task<bool> ExistsAsync(string name);
    }
}

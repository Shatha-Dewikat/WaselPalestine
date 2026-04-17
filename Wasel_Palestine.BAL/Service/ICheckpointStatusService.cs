using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;

namespace Wasel_Palestine.BAL.Service
{
    public interface ICheckpointStatusService
    {
        Task<List<CheckpointStatusResponse>> GetAllAsync();
        Task<CheckpointStatusResponse> CreateAsync(CreateCheckpointStatusRequest request);
    }
}

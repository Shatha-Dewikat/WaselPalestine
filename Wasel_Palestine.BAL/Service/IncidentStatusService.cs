using Mapster;
using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Repository;

namespace Wasel_Palestine.BLL.Service
{
    public class IncidentStatusService : IIncidentStatusService
    {
        private readonly IIncidentStatusRepository _repository;

        public IncidentStatusService(IIncidentStatusRepository repository)
        {
            _repository = repository;
        }

        public async Task<IncidentStatusResponse> CreateStatusAsync(
    IncidentStatusCreateRequest request,
    string userId)
        {
           
            var exists = await _repository.ExistsByNameAsync(request.Name);
            if (exists)
                throw new InvalidOperationException($"Status '{request.Name}' already exists.");

            var status = new IncidentStatus
            {
                Name = request.Name
            };

            var result = await _repository.AddAsync(status);

            return result.Adapt<IncidentStatusResponse>();
        }

        public async Task<IncidentStatusResponse> UpdateStatusAsync(
     int id,
     IncidentStatusUpdateRequest request,
     string userId)
        {
            var status = await _repository.GetByIdAsync(id);

            if (status == null)
                throw new KeyNotFoundException("Status not found");

            
            var exists = await _repository.ExistsByNameAsync(request.Name, excludeId: id);
            if (exists)
                throw new InvalidOperationException($"Status '{request.Name}' already exists.");

            status.Name = request.Name;

            await _repository.UpdateAsync(status);

            return status.Adapt<IncidentStatusResponse>();
        }

        public async Task DeleteStatusAsync(int id, string userId)
        {
            var status = await _repository.GetByIdAsync(id);

            if (status == null)
                throw new KeyNotFoundException("Status not found");

            await _repository.DeleteAsync(status);
        }

        public async Task<IncidentStatusResponse> GetStatusByIdAsync(int id)
        {
            var status = await _repository.GetByIdAsync(id);

            return status?.Adapt<IncidentStatusResponse>();
        }

        public async Task<List<IncidentStatusResponse>> GetAllStatusesAsync()
        {
            var statuses = await _repository.GetAllAsync();

            return statuses.Adapt<List<IncidentStatusResponse>>();
        }
    }
}

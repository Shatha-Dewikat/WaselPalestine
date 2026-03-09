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
    public class IncidentSeverityService : IIncidentSeverityService
    {
        private readonly IIncidentSeverityRepository _repository;

        public IncidentSeverityService(IIncidentSeverityRepository repository)
        {
            _repository = repository;
        }

        public async Task<IncidentSeverityResponse> CreateIncidentSeverityAsync(
            IncidentSeverityCreateRequest request,
            string userId)
        {
            var severity = new IncidentSeverity
            {
                Name = request.Name
            };

            var result = await _repository.AddAsync(severity);

            return result.Adapt<IncidentSeverityResponse>();
        }

        public async Task<IncidentSeverityResponse> UpdateIncidentSeverityAsync(
            int id,
            IncidentSeverityUpdateRequest request,
            string userId)
        {
            var severity = await _repository.GetByIdAsync(id);

            if (severity == null)
                throw new KeyNotFoundException("Severity not found");

            severity.Name = request.Name;

            await _repository.UpdateAsync(severity);

            return severity.Adapt<IncidentSeverityResponse>();
        }

        public async Task DeleteIncidentSeverityAsync(int id, string userId)
        {
            var severity = await _repository.GetByIdAsync(id);

            if (severity == null)
                throw new KeyNotFoundException("Severity not found");

            await _repository.DeleteAsync(severity);
        }

        public async Task<IncidentSeverityResponse> GetIncidentSeverityByIdAsync(int id)
        {
            var severity = await _repository.GetByIdAsync(id);

            return severity?.Adapt<IncidentSeverityResponse>();
        }

        public async Task<List<IncidentSeverityResponse>> GetAllIncidentSeveritiesAsync()
        {
            var severities = await _repository.GetAllAsync();

            return severities.Adapt<List<IncidentSeverityResponse>>();
        }
    }
}

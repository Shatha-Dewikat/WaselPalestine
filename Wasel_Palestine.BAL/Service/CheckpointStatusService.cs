using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Repository;

namespace Wasel_Palestine.BLL.Service
{
    public class CheckpointStatusService : ICheckpointStatusService
    {
        private readonly ICheckpointStatusRepository _repo;

        public CheckpointStatusService(ICheckpointStatusRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<CheckpointStatusResponse>> GetAllAsync()
        {
            var statuses = await _repo.GetAllAsync();

            return statuses.Select(s => new CheckpointStatusResponse
            {
                Id = s.Id,
                Name = s.Name
            }).ToList();
        }

        public async Task<CheckpointStatusResponse> CreateAsync(CreateCheckpointStatusRequest request)
        {
            var response = new CheckpointStatusResponse();

            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    response.Success = false;
                    response.Message = "Validation error";
                    response.Errors = new List<string> { "Status name is required" };
                    return response;
                }

                var exists = await _repo.ExistsAsync(request.Name);

                if (exists)
                {
                    response.Success = false;
                    response.Message = "Duplicate status";
                    response.Errors = new List<string> { "Status already exists" };
                    return response;
                }

                var status = new CheckpointStatus
                {
                    Name = request.Name.Trim()
                };

                var created = await _repo.CreateAsync(status);

                response.Success = true;
                response.Message = "Status created successfully";
                response.Id = created.Id;
                response.Name = created.Name;

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Something went wrong";
                response.Errors = new List<string> { ex.Message };

                return response;
            }
        }
    }
}

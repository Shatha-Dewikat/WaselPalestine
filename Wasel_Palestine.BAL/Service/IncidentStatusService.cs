using Mapster;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Repository;

namespace Wasel_Palestine.BLL.Service
{
    public class IncidentStatusService : IIncidentStatusService
    {
        private readonly IIncidentStatusRepository _repository;

        private readonly ApplicationDbContext _context;

        public IncidentStatusService(IIncidentStatusRepository repository, ApplicationDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<IncidentStatusResponse> CreateStatusAsync(
            IncidentStatusCreateRequest request,
            string userId,
            string ip,
            string userAgent)
        {
            if (await _repository.ExistsByNameAsync(request.Name))
                throw new InvalidOperationException($"Status '{request.Name}' already exists.");

            var status = new IncidentStatus { Name = request.Name };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _repository.AddAsync(status);

                await _context.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "Create",
                    EntityName = nameof(IncidentStatus),
                    EntityId = result.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Created status: {result.Name}",
                    IPAddress = ip,
                    UserAgent = userAgent
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return result.Adapt<IncidentStatusResponse>();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IncidentStatusResponse> UpdateStatusAsync(
            int id,
            IncidentStatusUpdateRequest request,
            string userId,
            string ip,
            string userAgent)
        {
            var status = await _repository.GetByIdAsync(id);
            if (status == null) throw new KeyNotFoundException("Status not found");
            if (await _repository.ExistsByNameAsync(request.Name, excludeId: id))
                throw new InvalidOperationException($"Status '{request.Name}' already exists.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                status.Name = request.Name;
                await _repository.UpdateAsync(status);

                await _context.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "Update",
                    EntityName = nameof(IncidentStatus),
                    EntityId = status.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Updated status to: {status.Name}",
                    IPAddress = ip,
                    UserAgent = userAgent
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return status.Adapt<IncidentStatusResponse>();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteStatusAsync(int id, string userId, string ip, string userAgent)
        {
            var status = await _repository.GetByIdAsync(id);
            if (status == null) throw new KeyNotFoundException("Status not found");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _repository.DeleteAsync(status);

                await _context.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "Delete",
                    EntityName = nameof(IncidentStatus),
                    EntityId = status.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Deleted status: {status.Name}",
                    IPAddress = ip,
                    UserAgent = userAgent
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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
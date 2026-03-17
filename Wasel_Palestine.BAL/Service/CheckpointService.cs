using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Repository;

namespace Wasel_Palestine.BLL.Service
{
    public class CheckpointService : ICheckpointService
    {
        private readonly ICheckpointRepository _repository;

        public CheckpointService(ICheckpointRepository repository)
        {
            _repository = repository;
        }

        public async Task<CheckpointResponse> CreateCheckpointAsync(CreateCheckpointRequest request, string userId, string ip, string userAgent)
        {
            var checkpoint = new Checkpoint
            {
                NameEn = request.NameEn,
                NameAr = request.NameAr,
                DescriptionEn = request.DescriptionEn,
                DescriptionAr = request.DescriptionAr,
                LocationId = request.LocationId,
                CurrentStatus = request.Status,
                EstimatedDelayMinutes = request.EstimatedDelayMinutes,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateCheckpointAsync(checkpoint);

            await _repository.AddStatusHistoryAsync(new CheckpointStatusHistory
            {
                CheckpointId = checkpoint.Id,
                Status = request.Status,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = userId
            });

            await _repository.AddAuditLogAsync(new AuditLog
            {
                UserId = userId,
                Action = "CREATE",
                EntityName = "Checkpoint",
                EntityId = checkpoint.Id,
                Timestamp = DateTime.UtcNow,
                Details = "Checkpoint created",
                IPAddress = ip,
                UserAgent = userAgent
            });

            return new CheckpointResponse
            {
                Id = checkpoint.Id,
                Name = checkpoint.NameEn,
                Description = checkpoint.DescriptionEn,
                Status = checkpoint.CurrentStatus,
                EstimatedDelayMinutes = checkpoint.EstimatedDelayMinutes,
                CreatedAt = checkpoint.CreatedAt
            };
        }

        public async Task<List<CheckpointResponse>> GetAllCheckpointsAsync(string lang)
        {
            var checkpoints = await _repository.GetAllCheckpointsAsync();

            return checkpoints.Select(c => new CheckpointResponse
            {
                Id = c.Id,
                Name = lang == "ar" ? c.NameAr : c.NameEn,
                Description = lang == "ar" ? c.DescriptionAr : c.DescriptionEn,
                Status = c.CurrentStatus,
                EstimatedDelayMinutes = c.EstimatedDelayMinutes,
                CreatedAt = c.CreatedAt
            }).ToList();
        }

        public async Task<CheckpointResponse> GetCheckpointByIdAsync(int id, string lang)
        {
            var checkpoint = await _repository.GetCheckpointByIdAsync(id);

            if (checkpoint == null)
                return null;

            return new CheckpointResponse
            {
                Id = checkpoint.Id,
                Name = lang == "ar" ? checkpoint.NameAr : checkpoint.NameEn,
                Description = lang == "ar" ? checkpoint.DescriptionAr : checkpoint.DescriptionEn,
                Status = checkpoint.CurrentStatus,
                EstimatedDelayMinutes = checkpoint.EstimatedDelayMinutes,
                CreatedAt = checkpoint.CreatedAt
            };
        }

        public async Task<bool> UpdateCheckpointAsync(int id, UpdateCheckpointRequest request, string userId, string ip, string userAgent)
        {
            var checkpoint = await _repository.GetCheckpointByIdAsync(id);

            if (checkpoint == null)
                return false;

            checkpoint.NameEn = request.NameEn;
            checkpoint.NameAr = request.NameAr;
            checkpoint.DescriptionEn = request.DescriptionEn;
            checkpoint.DescriptionAr = request.DescriptionAr;
            checkpoint.EstimatedDelayMinutes = request.EstimatedDelayMinutes;

            await _repository.UpdateCheckpointAsync(checkpoint);

            await _repository.AddAuditLogAsync(new AuditLog
            {
                UserId = userId,
                Action = "UPDATE",
                EntityName = "Checkpoint",
                EntityId = checkpoint.Id,
                Timestamp = DateTime.UtcNow,
                Details = "Checkpoint updated",
                IPAddress = ip,
                UserAgent = userAgent
            });

            return true;
        }

        public async Task<bool> DeleteCheckpointAsync(int id, string userId, string ip, string userAgent)
        {
            var checkpoint = await _repository.GetCheckpointByIdAsync(id);

            if (checkpoint == null)
                return false;

            checkpoint.DeletedAt = DateTime.UtcNow;

            await _repository.UpdateCheckpointAsync(checkpoint);

            await _repository.AddAuditLogAsync(new AuditLog
            {
                UserId = userId,
                Action = "DELETE",
                EntityName = "Checkpoint",
                EntityId = checkpoint.Id,
                Timestamp = DateTime.UtcNow,
                Details = "Checkpoint soft deleted",
                IPAddress = ip,
                UserAgent = userAgent
            });

            return true;
        }

        public async Task<bool> RestoreCheckpointAsync(int id, string userId, string ip, string userAgent)
        {
            var checkpoint = await _repository.GetCheckpointEvenDeletedAsync(id);

            if (checkpoint == null || checkpoint.DeletedAt == null)
                return false;

            checkpoint.DeletedAt = null;

            await _repository.UpdateCheckpointAsync(checkpoint);

            await _repository.AddAuditLogAsync(new AuditLog
            {
                UserId = userId,
                Action = "RESTORE",
                EntityName = "Checkpoint",
                EntityId = checkpoint.Id,
                Timestamp = DateTime.UtcNow,
                Details = "Checkpoint restored",
                IPAddress = ip,
                UserAgent = userAgent
            });

            return true;
        }

        public async Task<bool> ChangeStatusAsync(int id, ChangeCheckpointStatusRequest request, string userId, string ip, string userAgent)
        {
            var checkpoint = await _repository.GetCheckpointByIdAsync(id);

            if (checkpoint == null)
                return false;

            var oldStatus = checkpoint.CurrentStatus;

            checkpoint.CurrentStatus = request.Status;

            await _repository.UpdateCheckpointAsync(checkpoint);

            await _repository.AddStatusHistoryAsync(new CheckpointStatusHistory
            {
                CheckpointId = checkpoint.Id,
                Status = request.Status,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = userId
            });

            await _repository.AddAuditLogAsync(new AuditLog
            {
                UserId = userId,
                Action = "STATUS_CHANGE",
                EntityName = "Checkpoint",
                EntityId = checkpoint.Id,
                Timestamp = DateTime.UtcNow,
                Details = $"Status changed from {oldStatus} to {request.Status}",
                IPAddress = ip,
                UserAgent = userAgent
            });

            return true;
        }

        public async Task<List<CheckpointHistoryResponse>> GetCheckpointHistoryAsync(int checkpointId)
        {
            var history = await _repository.GetCheckpointHistoryAsync(checkpointId);

            return history.Select(h => new CheckpointHistoryResponse
            {
                OldStatus = null,
                NewStatus = h.Status,
                ChangedBy = h.ChangedByUserId,
                ChangedAt = h.ChangedAt
            }).ToList();
        }

        public async Task<List<CheckpointResponse>> GetFilteredCheckpointsAsync(CheckpointFilterRequest filter, string lang)
        {
            var checkpoints = await _repository.GetAllCheckpointsAsync();

            if (!string.IsNullOrEmpty(filter.Status))
                checkpoints = checkpoints.Where(c => c.CurrentStatus == filter.Status).ToList();

            if (filter.LocationId.HasValue)
                checkpoints = checkpoints.Where(c => c.LocationId == filter.LocationId.Value).ToList();

            return checkpoints.Select(c => new CheckpointResponse
            {
                Id = c.Id,
                Name = lang == "ar" ? c.NameAr : c.NameEn,
                Description = lang == "ar" ? c.DescriptionAr : c.DescriptionEn,
                Status = c.CurrentStatus,
                EstimatedDelayMinutes = c.EstimatedDelayMinutes,
                CreatedAt = c.CreatedAt
            }).ToList();
        }


         public async Task<List<CheckpointResponse>> GetPagedCheckpointsAsync(CheckPointPaginationRequest pagination, string lang)
    {
        var checkpoints = await _repository.GetAllCheckpointsAsync();
        var paged = checkpoints.Skip((pagination.PageNumber - 1) * pagination.PageSize)
                              .Take(pagination.PageSize)
                              .ToList();

        return paged.Select(c => new CheckpointResponse
        {
            Id = c.Id,
            Name = lang == "ar" ? c.NameAr : c.NameEn,
            Description = lang == "ar" ? c.DescriptionAr : c.DescriptionEn,
            Status = c.CurrentStatus,
            EstimatedDelayMinutes = c.EstimatedDelayMinutes,
            CreatedAt = c.CreatedAt
        }).ToList();
    }

    }
}

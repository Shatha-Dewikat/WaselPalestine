using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            try
            {
                // تحقق من وجود الـ Location
                var location = await _repository.GetLocationByIdAsync(request.LocationId);
                if (location == null)
                    return new CheckpointResponse
                    {
                        Success = false,
                        Message = "Invalid LocationId",
                        Errors = new List<string> { $"No Location found with Id = {request.LocationId}" }
                    };

                // تحقق من عدم وجود أي checkpoint بنفس الـ Location
                var existing = await _repository.GetAllCheckpointsAsync();
                var duplicate = existing.Any(c =>
                    c.LocationId == request.LocationId &&
                    c.DeletedAt == null
                );

                if (duplicate)
                    return new CheckpointResponse
                    {
                        Success = false,
                        Message = "Duplicate checkpoint",
                        Errors = new List<string> { "A checkpoint already exists in this location." }
                    };

                // إنشاء checkpoint
                var checkpoint = new Checkpoint
                {
                    NameEn = request.NameEn,
                    NameAr = request.NameAr,
                    DescriptionEn = request.DescriptionEn,
                    DescriptionAr = request.DescriptionAr,
                    LocationId = request.LocationId,
                    CurrentStatus = request.Status ?? "Pending",
                    EstimatedDelayMinutes = request.EstimatedDelayMinutes,
                    CreatedAt = DateTime.UtcNow
                };

                await _repository.CreateCheckpointAsync(checkpoint);

                // جلب الـ checkpoint بعد الإنشاء لضمان أن كل القيم موجودة
                var created = await _repository.GetCheckpointByIdAsync(checkpoint.Id);

                // سجل StatusHistory
                await _repository.AddStatusHistoryAsync(new CheckpointStatusHistory
                {
                    CheckpointId = created.Id,
                    OldStatus = "INITIAL",
                    NewStatus = created.CurrentStatus,
                    ChangedAt = DateTime.UtcNow,
                    ChangedByUserId = userId
                });

                // سجل Audit
                await _repository.AddAuditLogAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "CREATE",
                    EntityName = "Checkpoint",
                    EntityId = created.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = "Checkpoint created",
                    IPAddress = ip,
                    UserAgent = userAgent
                });

                // أرجع response مضبوط
                return new CheckpointResponse
                {
                    Success = true,
                    Message = "Checkpoint created successfully",
                    Id = created.Id,
                    Name = created.NameEn,
                    Description = created.DescriptionEn,
                    Status = created.CurrentStatus,
                    EstimatedDelayMinutes = created.EstimatedDelayMinutes,
                    CreatedAt = created.CreatedAt
                };
            }
            catch (Exception ex)
            {
                return new CheckpointResponse
                {
                    Success = false,
                    Message = "Error creating checkpoint",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
        public async Task<CheckpointResponse> UpdateCheckpointAsync(int id, UpdateCheckpointRequest request, string userId, string ip, string userAgent)
        {
            try
            {
                var checkpoint = await _repository.GetCheckpointByIdAsync(id);
                if (checkpoint == null)
                    return new CheckpointResponse { Success = false, Message = "Checkpoint not found" };

                // تحقق من التكرار
                var allCheckpoints = await _repository.GetAllCheckpointsAsync();
                var isDuplicate = allCheckpoints.Any(c =>
                    c.Id != id &&
                    c.LocationId == request.LocationId &&
                    (c.NameEn.Equals(request.NameEn, StringComparison.OrdinalIgnoreCase) ||
                     c.NameAr.Equals(request.NameAr, StringComparison.OrdinalIgnoreCase)) &&
                    c.DeletedAt == null);

                if (isDuplicate)
                    return new CheckpointResponse
                    {
                        Success = false,
                        Message = "Duplicate checkpoint",
                        Errors = new List<string> { "A checkpoint with the same name already exists in this location." }
                    };

                // احفظ القديم قبل التعديل
                var oldStatus = checkpoint.CurrentStatus;

                checkpoint.NameEn = request.NameEn;
                checkpoint.NameAr = request.NameAr;
                checkpoint.DescriptionEn = request.DescriptionEn;
                checkpoint.DescriptionAr = request.DescriptionAr;
                checkpoint.LocationId = request.LocationId;
                checkpoint.EstimatedDelayMinutes = request.EstimatedDelayMinutes;

                if (!string.IsNullOrEmpty(request.Status) && request.Status != oldStatus)
                {
                    checkpoint.CurrentStatus = request.Status;

                    // سجل StatusHistory
                    await _repository.AddStatusHistoryAsync(new CheckpointStatusHistory
                    {
                        CheckpointId = checkpoint.Id,
                        OldStatus = oldStatus,
                        NewStatus = checkpoint.CurrentStatus,
                        ChangedAt = DateTime.UtcNow,
                        ChangedByUserId = userId
                    });
                }

                await _repository.UpdateCheckpointAsync(checkpoint);

                // سجل Audit
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

                return new CheckpointResponse
                {
                    Success = true,
                    Message = "Checkpoint updated successfully",
                    Id = checkpoint.Id,
                    Name = checkpoint.NameEn,
                    Description = checkpoint.DescriptionEn,
                    Status = checkpoint.CurrentStatus,
                    EstimatedDelayMinutes = checkpoint.EstimatedDelayMinutes,
                    CreatedAt = checkpoint.CreatedAt
                };
            }
            catch (Exception ex)
            {
                return new CheckpointResponse
                {
                    Success = false,
                    Message = "Error updating checkpoint",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<CheckpointResponse> DeleteCheckpointAsync(int id, string userId, string ip, string userAgent)
        {
            try
            {
                var checkpoint = await _repository.GetCheckpointByIdAsync(id);
                if (checkpoint == null)
                    return new CheckpointResponse { Success = false, Message = "Checkpoint not found" };

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

                return new CheckpointResponse
                {
                    Success = true,
                    Message = "Checkpoint deleted successfully",
                    Id = checkpoint.Id,
                    Name = checkpoint.NameEn,
                    Status = checkpoint.CurrentStatus,
                    EstimatedDelayMinutes = checkpoint.EstimatedDelayMinutes,
                    CreatedAt = checkpoint.CreatedAt
                };
            }
            catch (Exception ex)
            {
                return new CheckpointResponse
                {
                    Success = false,
                    Message = "Error deleting checkpoint",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<CheckpointResponse> RestoreCheckpointAsync(int id, string userId, string ip, string userAgent)
        {
            try
            {
                var checkpoint = await _repository.GetCheckpointEvenDeletedAsync(id);
                if (checkpoint == null || checkpoint.DeletedAt == null)
                    return new CheckpointResponse { Success = false, Message = "Checkpoint not found or not deleted" };

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

                return new CheckpointResponse
                {
                    Success = true,
                    Message = "Checkpoint restored successfully",
                    Id = checkpoint.Id,
                    Name = checkpoint.NameEn,
                    Status = checkpoint.CurrentStatus,
                    EstimatedDelayMinutes = checkpoint.EstimatedDelayMinutes,
                    CreatedAt = checkpoint.CreatedAt
                };
            }
            catch (Exception ex)
            {
                return new CheckpointResponse
                {
                    Success = false,
                    Message = "Error restoring checkpoint",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<CheckpointResponse> ChangeStatusAsync(int id, ChangeCheckpointStatusRequest request, string userId, string ip, string userAgent)
        {
            try
            {
                var checkpoint = await _repository.GetCheckpointByIdAsync(id);
                if (checkpoint == null)
                    return new CheckpointResponse { Success = false, Message = "Checkpoint not found" };

                // تحقق من صحة الـ status (لو عندك جدول Status)
                var statusExists = await _repository.CheckpointStatusExistsAsync(request.Status);
                if (!statusExists)
                    return new CheckpointResponse
                    {
                        Success = false,
                        Message = "Invalid Status",
                        Errors = new List<string> { $"Status '{request.Status}' does not exist." }
                    };

                var oldStatus = checkpoint.CurrentStatus;
                checkpoint.CurrentStatus = request.Status;

                await _repository.UpdateCheckpointAsync(checkpoint);

                // سجل StatusHistory
                await _repository.AddStatusHistoryAsync(new CheckpointStatusHistory
                {
                    CheckpointId = checkpoint.Id,
                    OldStatus = oldStatus,
                    NewStatus = checkpoint.CurrentStatus,
                    ChangedAt = DateTime.UtcNow,
                    ChangedByUserId = userId
                });

                // سجل Audit
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

                return new CheckpointResponse
                {
                    Success = true,
                    Message = $"Status changed to {checkpoint.CurrentStatus}",
                    Id = checkpoint.Id,
                    Name = checkpoint.NameEn,
                    Description = checkpoint.DescriptionEn,
                    Status = checkpoint.CurrentStatus,
                    EstimatedDelayMinutes = checkpoint.EstimatedDelayMinutes,
                    CreatedAt = checkpoint.CreatedAt
                };
            }
            catch (Exception ex)
            {
                return new CheckpointResponse
                {
                    Success = false,
                    Message = "Error changing status",
                    Errors = new List<string> { ex.Message }
                };
            }
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
            var c = await _repository.GetCheckpointByIdAsync(id);
            if (c == null) return null;

            return new CheckpointResponse
            {
                Id = c.Id,
                Name = lang == "ar" ? c.NameAr : c.NameEn,
                Description = lang == "ar" ? c.DescriptionAr : c.DescriptionEn,
                Status = c.CurrentStatus,
                EstimatedDelayMinutes = c.EstimatedDelayMinutes,
                CreatedAt = c.CreatedAt
            };
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

        public async Task<List<CheckpointHistoryResponse>> GetCheckpointHistoryAsync(int checkpointId)
        {
            var history = await _repository.GetCheckpointHistoryAsync(checkpointId);

            return history.Select(h => new CheckpointHistoryResponse
            {
                OldStatus = h.OldStatus,
                NewStatus = h.NewStatus,
                ChangedBy = h.ChangedByUserId,
                ChangedAt = h.ChangedAt
            }).ToList();
        }
    }
}
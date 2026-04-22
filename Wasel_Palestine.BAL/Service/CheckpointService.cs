using Mapster;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Repository;

namespace Wasel_Palestine.BAL.Service
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
                var allCheckpoints = await _repository.GetAllCheckpointsAsync();

                var isDuplicate = allCheckpoints.Any(c =>
                    c.DeletedAt == null && 
                    (
                        ((c.NameEn.Equals(request.NameEn, StringComparison.OrdinalIgnoreCase) ||
                          c.NameAr.Equals(request.NameAr, StringComparison.OrdinalIgnoreCase))
                          && c.Location.City.Equals(request.City, StringComparison.OrdinalIgnoreCase))
                        ||
                        (Math.Abs(c.Location.Latitude - request.Latitude) < 0.0001m &&
                         Math.Abs(c.Location.Longitude - request.Longitude) < 0.0001m))
                    
                );

                if (isDuplicate)
                {
                    return new CheckpointResponse
                    {
                        Success = false,
                        Message = "هذا الحاجز موجود بالفعل في قاعدة البيانات بنفس الاسم أو الموقع.",
                        Errors = new List<string> { "Duplicate checkpoint detected: Name or Location already exists." }
                    };
                }

                var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

                var location = new DAL.Model.Location
                {
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Coordinates = geometryFactory.CreatePoint(new Coordinate((double)request.Longitude, (double)request.Latitude)),
                    AreaName = request.AreaName,
                    City = request.City,
                    CreatedAt = DateTime.UtcNow
                };

                var checkpoint = new Checkpoint
                {
                    NameEn = request.NameEn,
                    NameAr = request.NameAr,
                    DescriptionEn = request.DescriptionEn,
                    DescriptionAr = request.DescriptionAr,
                    Location = location,
                    CurrentStatus = request.Status ?? "Open",
                    EstimatedDelayMinutes = request.EstimatedDelayMinutes,
                    CreatedAt = DateTime.UtcNow
                };

                await _repository.CreateCheckpointAsync(checkpoint);

                await _repository.AddStatusHistoryAsync(new CheckpointStatusHistory
                {
                    CheckpointId = checkpoint.Id,
                    OldStatus = "INITIAL",
                    NewStatus = checkpoint.CurrentStatus,
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
                    Details = $"Checkpoint created: {checkpoint.NameEn} in {request.City}",
                    IPAddress = ip,
                    UserAgent = userAgent
                });

                return new CheckpointResponse
                {
                    Success = true,
                    Message = "Checkpoint created successfully",
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

               
                await _repository.AddStatusHistoryAsync(new CheckpointStatusHistory
                {
                    CheckpointId = checkpoint.Id,
                    OldStatus = oldStatus,
                    NewStatus = checkpoint.CurrentStatus,
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
            {
                checkpoints = checkpoints.Where(c =>
                    c.CurrentStatus != null &&
                    c.CurrentStatus.Equals(filter.Status, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (filter.LocationId.HasValue)
            {
                checkpoints = checkpoints.Where(c => c.LocationId == filter.LocationId.Value).ToList();
            }

            if (!string.IsNullOrEmpty(filter.City))
            {
                checkpoints = checkpoints.Where(c =>
                    c.Location != null &&
                    c.Location.City != null &&
                    c.Location.City.Contains(filter.City, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

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

        public async Task<bool> ProcessConfirmedReportAsync(int checkpointId, string newStatus, string userId, string ip, string userAgent)
        {
            try
            {
                var checkpoint = await _repository.GetCheckpointByIdAsync(checkpointId);
                if (checkpoint == null) return false;

                var oldStatus = checkpoint.CurrentStatus;
                if (oldStatus.Equals(newStatus, StringComparison.OrdinalIgnoreCase))
                    return true;

                var statusExists = await _repository.CheckpointStatusExistsAsync(newStatus);
                if (!statusExists) return false;

                checkpoint.CurrentStatus = newStatus;
                await _repository.UpdateCheckpointAsync(checkpoint);

                await _repository.AddStatusHistoryAsync(new CheckpointStatusHistory
                {
                    CheckpointId = checkpoint.Id,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    ChangedAt = DateTime.UtcNow,
                    ChangedByUserId = userId 
                });

                await _repository.AddAuditLogAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "STATUS_UPDATE_BY_REPORT",
                    EntityName = "Checkpoint",
                    EntityId = checkpoint.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Status updated automatically from {oldStatus} to {newStatus} via confirmed report.",
                    IPAddress = ip,
                    UserAgent = userAgent
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
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

        public async Task<List<CheckpointResponse>> GetNearbyCheckpointsAsync(double userLat, double userLon, double radiusInKm, string lang)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var userLocation = geometryFactory.CreatePoint(new Coordinate(userLon, userLat));

            var checkpoints = await _repository.GetAllCheckpointsAsync();

            return checkpoints
                .Where(c => c.Location != null && c.Location.Coordinates != null) 
                .Select(c => new
                {
                    Data = c,
                    Distance = c.Location.Coordinates.Distance(userLocation) * 111.32
                })
                .Where(x => x.Distance <= radiusInKm)
                .OrderBy(x => x.Distance)
                .Take(5)
                .Select(x => {
                   
                    var res = x.Data.Adapt<CheckpointResponse>();

                    res.Distance = Math.Round(x.Distance, 2);
                    res.Name = (lang == "ar") ? x.Data.NameAr : x.Data.NameEn;
                    res.Description = (lang == "ar") ? x.Data.DescriptionAr : x.Data.DescriptionEn;
                    res.Status = x.Data.CurrentStatus; 
                    res.Success = true;

                    return res;
                })
                .ToList();
        }
    }
}
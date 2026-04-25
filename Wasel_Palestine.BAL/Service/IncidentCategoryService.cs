using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.BLL.Service
{
    public class IncidentCategoryService : IIncidentCategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string SYSTEM_USER_ID = "SYSTEM_USER_ID_FROM_DB";
        private const string CategoriesCacheKey = "IncidentCategories_";

        public IncidentCategoryService(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        private async Task<string> GetValidUserIdAsync(string userId)
        {
            if (!string.IsNullOrEmpty(userId) && await _context.Users.AnyAsync(u => u.Id == userId))
                return userId;
            return SYSTEM_USER_ID;
        }

        private void ClearCategoriesCache()
        {
            _cache.Remove($"{CategoriesCacheKey}en");
            _cache.Remove($"{CategoriesCacheKey}ar");
        }

        public async Task<IncidentCategoryResponse> CreateIncidentCategoryAsync(
            IncidentCategoryCreateRequest request,
            string userId,
            string ip,
            string userAgent)
        {
            var actualUserId = await GetValidUserIdAsync(userId);

            if (await _context.IncidentCategories.AnyAsync(c => c.Name == request.Name || c.NameAr == request.NameAr))
                throw new ArgumentException("Category with same name already exists.");

            var category = new IncidentCategory
            {
                Name = request.Name,
                NameAr = request.NameAr
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.IncidentCategories.AddAsync(category);
                await _context.SaveChangesAsync();

                await _context.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = actualUserId,
                    Action = "Create",
                    EntityName = nameof(IncidentCategory),
                    EntityId = category.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Created category: {category.Name}/{category.NameAr}",
                    IPAddress = ip,
                    UserAgent = userAgent
                });
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                ClearCategoriesCache();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return new IncidentCategoryResponse
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task<IncidentCategoryResponse> UpdateIncidentCategoryAsync(
            int id,
            IncidentCategoryUpdateRequest request,
            string userId,
            string ip,
            string userAgent)
        {
            var actualUserId = await GetValidUserIdAsync(userId);
            var category = await _context.IncidentCategories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) throw new KeyNotFoundException("Category not found.");

            if (await _context.IncidentCategories.AnyAsync(c =>
                (c.Name == request.Name || c.NameAr == request.NameAr) && c.Id != id))
                throw new ArgumentException("Another category with same name exists.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                category.Name = request.Name;
                category.NameAr = request.NameAr;
                await _context.SaveChangesAsync();

                await _context.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = actualUserId,
                    Action = "Update",
                    EntityName = nameof(IncidentCategory),
                    EntityId = category.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Updated category to: {category.Name}/{category.NameAr}",
                    IPAddress = ip,
                    UserAgent = userAgent
                });
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                ClearCategoriesCache();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return new IncidentCategoryResponse
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task DeleteIncidentCategoryAsync(int id, string userId, string ip, string userAgent)
        {
            var actualUserId = await GetValidUserIdAsync(userId);
            var category = await _context.IncidentCategories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) throw new KeyNotFoundException("Category not found.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                category.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _context.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = actualUserId,
                    Action = "Delete",
                    EntityName = nameof(IncidentCategory),
                    EntityId = category.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Soft-deleted category: {category.Name}/{category.NameAr}",
                    IPAddress = ip,
                    UserAgent = userAgent
                });
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                ClearCategoriesCache();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RestoreIncidentCategoryAsync(int id, string userId, string ip, string userAgent)
        {
            var actualUserId = await GetValidUserIdAsync(userId);
            var category = await _context.IncidentCategories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) throw new KeyNotFoundException("Category not found.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                category.DeletedAt = null;
                await _context.SaveChangesAsync();

                await _context.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = actualUserId,
                    Action = "Restore",
                    EntityName = nameof(IncidentCategory),
                    EntityId = category.Id,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Restored category: {category.Name}/{category.NameAr}",
                    IPAddress = ip,
                    UserAgent = userAgent
                });
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                ClearCategoriesCache();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IncidentCategoryResponse> GetIncidentCategoryByIdAsync(int id, string lang = "en")
        {
            var category = await _context.IncidentCategories.FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
            if (category == null) return null;

            return new IncidentCategoryResponse
            {
                Id = category.Id,
                Name = lang == "ar" ? category.NameAr : category.Name
            };
        }

        public async Task<List<IncidentCategoryResponse>> GetAllIncidentCategoriesAsync(string lang = "en")
        {
            string cacheKey = $"{CategoriesCacheKey}{lang}";

            if (!_cache.TryGetValue(cacheKey, out List<IncidentCategoryResponse> cachedCategories))
            {
                var categoriesFromDb = await _context.IncidentCategories
                    .Where(c => c.DeletedAt == null)
                    .ToListAsync();

                cachedCategories = categoriesFromDb.Select(c => new IncidentCategoryResponse
                {
                    Id = c.Id,
                    Name = lang == "ar" ? c.NameAr : c.Name
                }).ToList();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));

                _cache.Set(cacheKey, cachedCategories, cacheOptions);
            }

            return cachedCategories;
        }
    }
}
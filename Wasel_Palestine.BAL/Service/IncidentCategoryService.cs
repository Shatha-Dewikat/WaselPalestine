using Microsoft.EntityFrameworkCore;
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

        public IncidentCategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IncidentCategoryResponse> CreateIncidentCategoryAsync(IncidentCategoryCreateRequest request, string userId)
        {
            if (await _context.IncidentCategories.AnyAsync(c =>
                c.Name == request.Name || c.NameAr == request.NameAr))
                throw new ArgumentException("Category with same name already exists.");

            var category = new IncidentCategory
            {
                Name = request.Name,
                NameAr = request.NameAr
            };

            await _context.IncidentCategories.AddAsync(category);
            await _context.SaveChangesAsync();

          
            await _context.AuditLogs.AddAsync(new AuditLog
            {
                UserId = userId,
                Action = "Create",
                EntityName = nameof(IncidentCategory),
                EntityId = category.Id,
                Timestamp = DateTime.UtcNow,
                Details = $"Created category: {category.Name}/{category.NameAr}",
                IPAddress = "", // يمكن تمرير من Controller
                UserAgent = ""
            });
            await _context.SaveChangesAsync();

            return new IncidentCategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                NameAr = category.NameAr
            };
        }

        public async Task<IncidentCategoryResponse> UpdateIncidentCategoryAsync(int id, IncidentCategoryUpdateRequest request, string userId)
        {
            var category = await _context.IncidentCategories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) throw new KeyNotFoundException("Category not found.");

            if (await _context.IncidentCategories.AnyAsync(c =>
                (c.Name == request.Name || c.NameAr == request.NameAr) && c.Id != id))
                throw new ArgumentException("Another category with same name exists.");

            category.Name = request.Name;
            category.NameAr = request.NameAr;
            await _context.SaveChangesAsync();

           
            await _context.AuditLogs.AddAsync(new AuditLog
            {
                UserId = userId,
                Action = "Update",
                EntityName = nameof(IncidentCategory),
                EntityId = category.Id,
                Timestamp = DateTime.UtcNow,
                Details = $"Updated category to: {category.Name}/{category.NameAr}",
                IPAddress = "",
                UserAgent = ""
            });
            await _context.SaveChangesAsync();

            return new IncidentCategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                NameAr = category.NameAr
            };
        }

        public async Task DeleteIncidentCategoryAsync(int id, string userId)
        {
            var category = await _context.IncidentCategories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) throw new KeyNotFoundException("Category not found.");

            category.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

           
            await _context.AuditLogs.AddAsync(new AuditLog
            {
                UserId = userId,
                Action = "Delete",
                EntityName = nameof(IncidentCategory),
                EntityId = category.Id,
                Timestamp = DateTime.UtcNow,
                Details = $"Soft-deleted category: {category.Name}/{category.NameAr}",
                IPAddress = "",
                UserAgent = ""
            });
            await _context.SaveChangesAsync();
        }

        public async Task RestoreIncidentCategoryAsync(int id, string userId)
        {
            var category = await _context.IncidentCategories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) throw new KeyNotFoundException("Category not found.");

            category.DeletedAt = null;
            await _context.SaveChangesAsync();

           
            await _context.AuditLogs.AddAsync(new AuditLog
            {
                UserId = userId,
                Action = "Restore",
                EntityName = nameof(IncidentCategory),
                EntityId = category.Id,
                Timestamp = DateTime.UtcNow,
                Details = $"Restored category: {category.Name}/{category.NameAr}",
                IPAddress = "",
                UserAgent = ""
            });
            await _context.SaveChangesAsync();
        }

        public async Task<IncidentCategoryResponse> GetIncidentCategoryByIdAsync(int id, string lang = "en")
        {
            var category = await _context.IncidentCategories.FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
            if (category == null) return null;

            return new IncidentCategoryResponse
            {
                Id = category.Id,
                Name = lang == "ar" ? category.NameAr : category.Name,
                NameAr = category.NameAr
            };
        }

        public async Task<List<IncidentCategoryResponse>> GetAllIncidentCategoriesAsync(string lang = "en")
        {
            var categories = await _context.IncidentCategories
                .Where(c => c.DeletedAt == null)
                .ToListAsync();

            return categories.Select(c => new IncidentCategoryResponse
            {
                Id = c.Id,
                Name = lang == "ar" ? c.NameAr : c.Name,
                NameAr = c.NameAr
            }).ToList();
        }
    }
}
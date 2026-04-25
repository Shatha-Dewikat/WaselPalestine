using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public class IncidentCategoryRepository : IIncidentCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public IncidentCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IncidentCategory> AddAsync(IncidentCategory category)
        {
            await _context.IncidentCategories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<List<IncidentCategory>> GetAllAsync()
        {
            return await _context.IncidentCategories
                .Where(c => c.DeletedAt == null)
                .ToListAsync();
        }

        public async Task<IncidentCategory> GetByIdAsync(int id)
        {
            return await _context.IncidentCategories
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
        }

        public async Task<IncidentCategory> UpdateAsync(IncidentCategory category)
        {
            _context.IncidentCategories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task DeleteAsync(IncidentCategory category)
        {
            category.DeletedAt = DateTime.UtcNow;
            _context.IncidentCategories.Update(category);
            await _context.SaveChangesAsync();
        }
    }
}

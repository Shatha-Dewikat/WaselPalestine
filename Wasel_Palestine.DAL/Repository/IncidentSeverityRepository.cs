using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public class IncidentSeverityRepository : IIncidentSeverityRepository
    {
        private readonly ApplicationDbContext _context;

        public IncidentSeverityRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            return await _context.IncidentSeverities
                .AnyAsync(s => s.Name.ToLower() == name.ToLower() && (!excludeId.HasValue || s.Id != excludeId.Value));
        }
        public async Task<IncidentSeverity> AddAsync(IncidentSeverity severity)
        {
            _context.IncidentSeverities.Add(severity);
            await _context.SaveChangesAsync();
            return severity;
        }

        public async Task<IncidentSeverity> UpdateAsync(IncidentSeverity severity)
        {
            _context.IncidentSeverities.Update(severity);
            await _context.SaveChangesAsync();
            return severity;
        }

        public async Task DeleteAsync(IncidentSeverity severity)
        {
            _context.IncidentSeverities.Remove(severity);
            await _context.SaveChangesAsync();
        }

        public async Task<IncidentSeverity> GetByIdAsync(int id)
        {
            return await _context.IncidentSeverities.FindAsync(id);
        }

        public async Task<List<IncidentSeverity>> GetAllAsync()
        {
            return await _context.IncidentSeverities.ToListAsync();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public class IncidentStatusRepository : IIncidentStatusRepository
    {
        private readonly ApplicationDbContext _context;

        public IncidentStatusRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IncidentStatus> AddAsync(IncidentStatus status)
        {
            await _context.IncidentStatuses.AddAsync(status);
            await _context.SaveChangesAsync();
            return status;
        }

        public async Task<IncidentStatus> UpdateAsync(IncidentStatus status)
        {
            _context.IncidentStatuses.Update(status);
            await _context.SaveChangesAsync();
            return status;
        }

        public async Task DeleteAsync(IncidentStatus status)
        {
            _context.IncidentStatuses.Remove(status);
            await _context.SaveChangesAsync();
        }

        public async Task<IncidentStatus> GetByIdAsync(int id)
        {
            return await _context.IncidentStatuses.FindAsync(id);
        }

        public async Task<List<IncidentStatus>> GetAllAsync()
        {
            return await _context.IncidentStatuses.ToListAsync();
        }
    }
}

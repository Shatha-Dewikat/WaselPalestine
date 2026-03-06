using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public class IncidentRepository : IIncidentRepository
    {
        private readonly ApplicationDbContext _context;

        public IncidentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Incident incident)
        {
            _context.Incidents.Add(incident);
            await _context.SaveChangesAsync();
        }

        public async Task<Incident?> GetByIdAsync(int id)
        {
            return await _context.Incidents
                .Include(i => i.Location)
                .Include(i => i.Category)
                .Include(i => i.Severity)
                .Include(i => i.Status)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task UpdateAsync(Incident incident)
        {
            _context.Incidents.Update(incident);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Incident>> GetAllAsync()
             {
            return await _context.Incidents
              .Include(i => i.Category)
              .Include(i => i.Severity)
              .Include(i => i.Status)
              .Include(i => i.Location)
              .ToListAsync();
        }

        public async Task DeleteAsync(Incident incident)
        {
            _context.Incidents.Remove(incident);
            if (incident != null)
            {
                _context.Incidents.Remove(incident);
                await _context.SaveChangesAsync();
            }
        }
    }
}

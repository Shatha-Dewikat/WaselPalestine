using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public class IncidentMediaRepository : IIncidentMediaRepository
    {
        private readonly ApplicationDbContext _context;

        public IncidentMediaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IncidentMedia> AddAsync(IncidentMedia media)
        {
            await _context.IncidentMedias.AddAsync(media);
            await _context.SaveChangesAsync();
            return media;
        }

        public async Task DeleteAsync(IncidentMedia media)
        {
            _context.IncidentMedias.Remove(media);
            await _context.SaveChangesAsync();
        }

        public async Task<List<IncidentMedia>> GetByIncidentIdAsync(int incidentId)
        {
            return await _context.IncidentMedias
                .Where(m => m.IncidentId == incidentId)
                .ToListAsync();
        }

        public async Task<IncidentMedia> GetByIdAsync(int id)
        {
            return await _context.IncidentMedias.FindAsync(id);
        }
    }
}

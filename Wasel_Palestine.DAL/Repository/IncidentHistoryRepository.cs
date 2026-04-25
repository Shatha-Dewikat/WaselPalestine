using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public class IncidentHistoryRepository : IIncidentHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public IncidentHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(IncidentHistory history)
        {
            await _context.IncidentHistories.AddAsync(history);
            await _context.SaveChangesAsync();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public class CheckpointStatusRepository : ICheckpointStatusRepository
    {
        private readonly ApplicationDbContext _context;

        public CheckpointStatusRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CheckpointStatus>> GetAllAsync()
        {
            return await _context.CheckpointStatuses
                .Where(s => s.IsActive)
                .ToListAsync();
        }

        public async Task<CheckpointStatus> CreateAsync(CheckpointStatus status)
        {
            _context.CheckpointStatuses.Add(status);
            await _context.SaveChangesAsync();
            return status;
        }
        public async Task<bool> ExistsAsync(string name)
        {
            return await _context.CheckpointStatuses
                .AnyAsync(s => s.Name.ToLower() == name.ToLower() && s.IsActive);
        }
    }
}

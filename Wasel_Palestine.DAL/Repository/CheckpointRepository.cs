using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public class CheckpointRepository : ICheckpointRepository
    {
        private readonly ApplicationDbContext _context;

        public CheckpointRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAuditLogAsync(AuditLog log)
        {
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task AddStatusHistoryAsync(CheckpointStatusHistory history)
        {
            _context.CheckpointStatusHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<Checkpoint> CreateCheckpointAsync(Checkpoint checkpoint)
        {
            _context.Checkpoints.Add(checkpoint);
           await _context.SaveChangesAsync();
            return checkpoint;
        }

        public async Task<List<Checkpoint>> GetAllCheckpointsAsync()
        {
            return await _context.Checkpoints
               .Where(c => c.DeletedAt == null)
               .ToListAsync();
        }

        public async Task<Checkpoint> GetCheckpointByIdAsync(int id)
        {
            return await _context.Checkpoints
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
        }

        public async Task<Checkpoint> GetCheckpointEvenDeletedAsync(int id)
        {
            return await _context.Checkpoints
                 .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<CheckpointStatusHistory>> GetCheckpointHistoryAsync(int checkpointId)
        {
            return await _context.CheckpointStatusHistories
               .Where(h => h.CheckpointId == checkpointId)
               .OrderByDescending(h => h.ChangedAt)
               .ToListAsync();
        }

        public async Task UpdateCheckpointAsync(Checkpoint checkpoint)
        {
            _context.Checkpoints.Update(checkpoint);
            await _context.SaveChangesAsync();
        }
    }
}
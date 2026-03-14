using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public class AlertRepository : IAlertRepository
    {
        private readonly ApplicationDbContext _context;

        public AlertRepository(ApplicationDbContext context)
        {
            _context = context;
        }

       
        public async Task<Alert> CreateAsync(Alert alert)
        {
            var result = await _context.Alerts.AddAsync(alert);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

       
        public async Task<List<Alert>> GetAllAsync()
        {
            return await _context.Alerts
                .Include(a => a.Recipients)
                .Include(a => a.AlertHistories)
                .ToListAsync();
        }

  
        public async Task<Alert> GetByIdAsync(int id)
        {
            return await _context.Alerts
                .Include(a => a.Recipients)
                .Include(a => a.AlertHistories)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        
        public async Task<Alert> UpdateAsync(Alert alert)
        {
            var existingAlert = await _context.Alerts.FindAsync(alert.Id);
            if (existingAlert == null) return null;

          
            existingAlert.IncidentId = alert.IncidentId;

            _context.Alerts.Update(existingAlert);
            await _context.SaveChangesAsync();
            return existingAlert;
        }

       
        public async Task DeleteAsync(int id)
        {
            var alert = await _context.Alerts.FindAsync(id);
            if (alert != null)
            {
                _context.Alerts.Remove(alert);
                await _context.SaveChangesAsync();
            }
        }
    }
}
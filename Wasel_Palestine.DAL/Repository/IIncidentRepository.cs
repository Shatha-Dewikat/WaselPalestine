using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public interface IIncidentRepository
    {
        Task AddAsync(Incident incident);
        Task UpdateAsync(Incident incident);
        Task<Incident?> GetByIdAsync(int id);
        Task<List<Incident>> GetAllAsync();
        Task DeleteAsync(Incident incident);
    }
}

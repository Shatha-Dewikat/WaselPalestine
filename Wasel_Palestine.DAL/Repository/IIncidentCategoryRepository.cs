using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public interface IIncidentCategoryRepository
    {
        Task<IncidentCategory> AddAsync(IncidentCategory category);
        Task<IncidentCategory> GetByIdAsync(int id);
        Task<List<IncidentCategory>> GetAllAsync();
        Task<IncidentCategory> UpdateAsync(IncidentCategory category);
        Task DeleteAsync(IncidentCategory category);
    }
}

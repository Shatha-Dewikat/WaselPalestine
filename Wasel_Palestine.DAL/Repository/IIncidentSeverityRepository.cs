using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public interface IIncidentSeverityRepository
    {
        Task<IncidentSeverity> AddAsync(IncidentSeverity severity);

        Task<IncidentSeverity> UpdateAsync(IncidentSeverity severity);

        Task DeleteAsync(IncidentSeverity severity);

        Task<IncidentSeverity> GetByIdAsync(int id);

        Task<List<IncidentSeverity>> GetAllAsync();
    }
}

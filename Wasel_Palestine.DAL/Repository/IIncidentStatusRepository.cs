using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public interface IIncidentStatusRepository
    {
        Task<IncidentStatus> AddAsync(IncidentStatus status);

        Task<IncidentStatus> UpdateAsync(IncidentStatus status);

        Task DeleteAsync(IncidentStatus status);

        Task<IncidentStatus> GetByIdAsync(int id);

        Task<List<IncidentStatus>> GetAllAsync();
    }
}

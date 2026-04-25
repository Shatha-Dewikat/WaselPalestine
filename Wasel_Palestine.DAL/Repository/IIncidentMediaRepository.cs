using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Repository
{
    public interface IIncidentMediaRepository
    {
        Task<IncidentMedia> AddAsync(IncidentMedia media);
        Task DeleteAsync(IncidentMedia media);
        Task<List<IncidentMedia>> GetByIncidentIdAsync(int incidentId);
        Task<IncidentMedia> GetByIdAsync(int id);
        Task GetAllAsync();
    }
}

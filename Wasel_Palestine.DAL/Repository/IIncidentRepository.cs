using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Request;
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
        Task<List<Incident>> GetFilteredAsync(IncidentFilterRequest filter);
        Task<List<Incident>> GetPagedAsync(PaginationRequest paginationRequest);
        Task<List<Incident>> GetFilteredPagedAsync(IncidentQueryRequest request);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;

namespace Wasel_Palestine.BLL.Service
{
    public interface IIncidentService
    {
        Task<IncidentResponse> CreateIncidentAsync(CreateIncidentRequest request, string userId);
        Task<IncidentResponse> UpdateIncidentAsync(int id, UpdateIncidentRequest request, string userId);
        Task<IncidentResponse> DeleteIncidentAsync(int id, string userId);
        Task<IncidentResponse> GetIncidentByIdAsync(int id, string lang = "en");
        Task<List<IncidentResponse>> GetIncidentAllAsync(string lang = "en");
        Task<List<IncidentHistoryResponse>> GetIncidentHistoryAsync(int incidentId);
        Task<List<IncidentResponse>> GetFilteredIncidentsAsync(IncidentFilterRequest filter, string lang = "en");
        Task<List<IncidentResponse>> GetPagedIncidentsAsync(PaginationRequest paginationRequest, string lang = "en");
        Task<List<IncidentResponse>> GetFilteredPagedIncidentsAsync(IncidentQueryRequest request, string lang = "en");
    }
}
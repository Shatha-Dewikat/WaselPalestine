using System.Collections.Generic;
using System.Threading.Tasks;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.BLL.Service
{
    public interface IIncidentService
    {
        Task<IncidentResponse> CreateIncidentAsync(CreateIncidentRequest request, string userId = null);
        Task<IncidentResponse> UpdateIncidentAsync(int id, UpdateIncidentRequest request, string userId = null);
        Task<IncidentResponse> DeleteIncidentAsync(int id, string userId = null);
        Task<IncidentResponse> GetIncidentByIdAsync(int id, string lang = "en");
        Task<List<IncidentResponse>> GetIncidentAllAsync(string lang = "en");
        Task<List<IncidentHistoryResponse>> GetIncidentHistoryAsync(int incidentId);
        Task<List<IncidentResponse>> GetFilteredIncidentsAsync(IncidentFilterRequest filter, string lang = "en");
        Task<List<IncidentResponse>> GetPagedIncidentsAsync(PaginationRequest paginationRequest, string lang = "en");
        Task<List<IncidentResponse>> GetFilteredPagedIncidentsAsync(IncidentQueryRequest request, string lang = "en");
        Task<SimpleResponse> VerifyIncidentAsync(int incidentId, string userId);
        Task<SimpleResponse> CloseIncidentAsync(int incidentId, string userId);
        Task<SimpleResponse> ResolveIncidentAsync(int incidentId, string userId);
        Task<List<IncidentResponse>> GetIncidentsByCheckpointIdAsync(int checkpointId, string lang = "en");
        Task<List<CityIncidentStats>> GetDashboardStatsAsync();
        Task AutoCreateWeatherIncidentAsync(double lat, double lon);
        Task ProcessWeatherIncidentsAsync();

    }
}
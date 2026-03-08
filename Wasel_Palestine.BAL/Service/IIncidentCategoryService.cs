using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using System.Threading.Tasks;

namespace Wasel_Palestine.BLL.Service
{
    public interface IIncidentCategoryService
    {
        Task<IncidentCategoryResponse> CreateIncidentCategoryAsync(IncidentCategoryCreateRequest request, string userId);
        Task<IncidentCategoryResponse> UpdateIncidentCategoryAsync(int id, IncidentCategoryUpdateRequest request, string userId);
        Task DeleteIncidentCategoryAsync(int id, string userId);
        Task RestoreIncidentCategoryAsync(int id, string userId);
        Task<IncidentCategoryResponse> GetIncidentCategoryByIdAsync(int id, string lang = "en");
        Task<List<IncidentCategoryResponse>> GetAllIncidentCategoriesAsync(string lang = "en");
    }
}
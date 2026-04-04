using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;

namespace Wasel_Palestine.BLL.Service
{
    public interface IIncidentCategoryService
    {
        Task<IncidentCategoryResponse> CreateIncidentCategoryAsync(
            IncidentCategoryCreateRequest request,
            string userId,
            string ip,
            string userAgent);

        Task<IncidentCategoryResponse> UpdateIncidentCategoryAsync(
            int id,
            IncidentCategoryUpdateRequest request,
            string userId,
            string ip,
            string userAgent);

        Task DeleteIncidentCategoryAsync(
            int id,
            string userId,
            string ip,
            string userAgent);

        Task RestoreIncidentCategoryAsync(
            int id,
            string userId,
            string ip,
            string userAgent);

        Task<IncidentCategoryResponse> GetIncidentCategoryByIdAsync(
            int id,
            string lang = "en");

        Task<List<IncidentCategoryResponse>> GetAllIncidentCategoriesAsync(
            string lang = "en");
    }
}
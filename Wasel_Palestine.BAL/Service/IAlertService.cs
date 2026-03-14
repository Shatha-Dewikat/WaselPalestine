using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;

namespace Wasel_Palestine.BLL.Service
{
    public interface IAlertService
    {
        Task<AlertResponse> CreateAlertAsync(AlertCreateRequest request, string userId, string ip, string userAgent);
        Task<AlertResponse> UpdateAlertAsync(AlertUpdateRequest request, string userId, string ip, string userAgent);
        Task DeleteAlertAsync(int id, string userId, string ip, string userAgent);
        Task<AlertResponse> GetAlertByIdAsync(int id, string lang);
        Task<List<AlertResponse>> GetAllAlertsAsync(string lang);
    }
}

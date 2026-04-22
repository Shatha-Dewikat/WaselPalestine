using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.BAL.DTOs;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.BAL.Service
{
    public interface IAlertService
    {
        Task<object> GetAlertsStatisticsAsync();
        Task<bool> UnsubscribeFromAlertsAsync(int alertId, string userId);
        Task<string> SubscribeToAlertAsync(SubscribeAlertDto subscriptionDto);
        Task<AlertResponse> CreateAlertAsync(AlertCreateRequest request, string userId, string ip, string userAgent);
        Task<AlertResponse> UpdateAlertAsync(AlertUpdateRequest request, string userId, string ip, string userAgent);
        Task DeleteAlertAsync(int id, string userId, string ip, string userAgent);
        Task<AlertResponse> GetAlertByIdAsync(int id, string lang);
        Task<List<AlertResponse>> GetAllAlertsAsync(string lang);
        Task<List<AlertHistory>> GetAlertHistoryAsync(int alertId);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;

namespace Wasel_Palestine.BAL.Service
{
    public interface IIncidentMediaService
    {
        Task<IncidentMediaResponse> AddMediaAsync(IncidentMediaCreateRequest request);
        Task DeleteMediaAsync(int id);
        Task<List<IncidentMediaResponse>> GetByIncidentIdAsync(int incidentId);
    }
}

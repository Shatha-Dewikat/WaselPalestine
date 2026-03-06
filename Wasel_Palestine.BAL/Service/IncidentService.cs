using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Repository;

namespace Wasel_Palestine.BLL.Service
{
    public class IncidentService : IIncidentService
    {
        private readonly IIncidentRepository _incidentRepo;

        public IncidentService(IIncidentRepository incidentRepo)
        {
            _incidentRepo = incidentRepo;
        }


        public async Task<IncidentResponse> UpdateIncidentAsync(int id, UpdateIncidentRequest request)
        {
            try
            {
                var incident = await _incidentRepo.GetByIdAsync(id);
                if (incident is null)
                    return new IncidentResponse { Success = false, Message = "Incident not found" };

                incident.Title = string.IsNullOrEmpty(request.Title) ? incident.Title : request.Title;
                incident.Description = string.IsNullOrEmpty(request.Description) ? incident.Description : request.Description;
                incident.CategoryId = request.CategoryId ?? incident.CategoryId;
                incident.SeverityId = request.SeverityId ?? incident.SeverityId;
                incident.StatusId = request.StatusId ?? incident.StatusId;

                if (incident.Location == null) incident.Location = new Location();
                incident.Location.Latitude = request.Latitude.HasValue ? (decimal)request.Latitude.Value : incident.Location.Latitude;
                incident.Location.Longitude = request.Longitude.HasValue ? (decimal)request.Longitude.Value : incident.Location.Longitude;

                await _incidentRepo.UpdateAsync(incident);

                return new IncidentResponse
                {
                    Id = incident.Id,
                    Title = incident.Title,
                    Description = incident.Description,
                    Category = incident.Category?.Name,
                    Severity = incident.Severity?.Name,
                    Status = incident.Status?.Name,
                    Latitude = (double)incident.Location.Latitude,
                    Longitude = (double)incident.Location.Longitude,
                    CreatedAt = incident.CreatedAt
                };
            }
            catch (Exception ex)
            {
                return new IncidentResponse { Success = false, Message = "An error occurred while updating the incident." };
            }
        }
        public async Task<IncidentResponse> CreateIncidentAsync(CreateIncidentRequest request, string userId)
        {
            var location = new Location
            {
                Latitude = (decimal)request.Latitude,
                Longitude = (decimal)request.Longitude
            };

            var incident = new Incident
            {
                Title = request.Title,
                Description = request.Description,
                CategoryId = request.CategoryId,
                SeverityId = request.SeverityId,
                StatusId = 1, 
                Location = location,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _incidentRepo.AddAsync(incident);

            return new IncidentResponse
            {
                Id = incident.Id,
                Title = incident.Title,
                Description = incident.Description,
                Category = incident.Category?.Name,
                Severity = incident.Severity?.Name,
                Status = incident.Status?.Name,
                Latitude = (double)location.Latitude,
                Longitude = (double)location.Longitude,
                CreatedAt = incident.CreatedAt
            };
        }

        public async Task<IncidentResponse> GetIncidentByIdAsync(int id)
        {
            var incident = await _incidentRepo.GetByIdAsync(id);
            if (incident == null)
            {
                return new IncidentResponse { Success = false, Message = "Incident not found" };
            }
            var response = new IncidentResponse
            {
                Id = incident.Id,
                Title = incident.Title,
                Description = incident.Description,
                Category = incident.Category?.Name,
                Severity = incident.Severity?.Name,
                Status = incident.Status?.Name,
                Latitude = (double)incident.Location.Latitude,
                Longitude = (double)incident.Location.Longitude,
                CreatedAt = incident.CreatedAt
            };
            return response;
        }

        public async Task<List<IncidentResponse>> GetIncidentAllAsync()
        {
            var incidents = await _incidentRepo.GetAllAsync();
            var response = new List<IncidentResponse>();
            foreach (var incident in incidents)
            {
                response.Add(new IncidentResponse
                {
                    Id = incident.Id,
                    Title = incident.Title,
                    Description = incident.Description,
                    Category = incident.Category?.Name,
                    Severity = incident.Severity?.Name,
                    Status = incident.Status?.Name,
                    Latitude = (double)incident.Location.Latitude,
                    Longitude = (double)incident.Location.Longitude,
                    CreatedAt = incident.CreatedAt
                });
            }
            return response;
        }

        public async Task<IncidentResponse> DeleteIncidentAsync(int id)
        {
            var incident = await _incidentRepo.GetByIdAsync(id);
            if (incident == null)
            {
                return new IncidentResponse { Success = false, Message = "Incident not found" };
            }
            await _incidentRepo.DeleteAsync(incident);
            return new IncidentResponse { Success = true, Message = "Incident deleted successfully" };
        }
    }
}

using Mapster;
using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.DAL.DTO.Request;
using Wasel_Palestine.DAL.DTO.Response;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Repository;
using Microsoft.AspNetCore.Hosting;

namespace Wasel_Palestine.BLL.Service
{
    public class IncidentMediaService : IIncidentMediaService
    {
        private readonly IIncidentMediaRepository _repository;
        private readonly IFileService _fileService;

        public IncidentMediaService(IIncidentMediaRepository repository, IFileService fileService)
        {
            _repository = repository;
            _fileService = fileService;
        }

        public async Task<IncidentMediaResponse> AddMediaAsync(IncidentMediaCreateRequest request)
        {
            var file = request.File;
            var fileName = await _fileService.UploadAsync(request.File);
            if (fileName == null)
                throw new Exception("No file uploaded");
            var baseUrl = "http://localhost:5034";
            var media = new IncidentMedia
            {
                IncidentId = request.IncidentId,
                FileName = fileName,
                Url = $"{baseUrl}/images/{fileName}",
                ContentType = file.ContentType,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _repository.AddAsync(media);
            return result.Adapt<IncidentMediaResponse>();
        }

        public async Task DeleteMediaAsync(int id)
        {
            var media = await _repository.GetByIdAsync(id);
            if (media == null)
                throw new KeyNotFoundException("Media not found");

            
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", media.Url.TrimStart('/'));
            if (File.Exists(path))
                File.Delete(path);

            await _repository.DeleteAsync(media);
        }

        public async Task<List<IncidentMediaResponse>> GetByIncidentIdAsync(int incidentId)
        {
            var list = await _repository.GetByIncidentIdAsync(incidentId);
            return list.Adapt<List<IncidentMediaResponse>>();
        }
    }
}

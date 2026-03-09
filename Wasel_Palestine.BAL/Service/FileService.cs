using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.BLL.Service
{
    public class FileService : IFileService
    {
        public async Task<string?> UploadAsync(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                using (var stream = File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }

                return fileName;
            }

            return null;
        }
    }
}

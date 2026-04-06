using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.BAL.Service
{
    public interface IFileService
    {
        Task<string?> UploadAsync(IFormFile file);
    }
}

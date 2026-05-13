using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IBlobService
    {
        Task<string> UploadImageAsync(IFormFile file, string containerName);
    }
}

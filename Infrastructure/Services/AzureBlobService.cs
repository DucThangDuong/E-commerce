using Application.IServices;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class AzureBlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        public AzureBlobService(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("AzureStorageAccount") ?? "";
            _blobServiceClient =new BlobServiceClient(connectionString);
        }
        public async Task<string> UploadImageAsync(IFormFile file, string containerName)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is null or empty.");
            }
            const long fileSizeLimit = 5 * 1024 * 1024; 
            if(file.Length > fileSizeLimit)
            {
                throw new ArgumentException("File size exceeds the limit of 5 MB.");
            }
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName.ToLower());
            string fileExtension = Path.GetExtension(file.FileName);
            string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var blobClient = containerClient.GetBlobClient(uniqueFileName);
            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            };
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });
            }
            return blobClient.Uri.ToString();
        }
    }
}

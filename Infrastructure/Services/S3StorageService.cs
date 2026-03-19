using Amazon.S3;
using Amazon.S3.Model;
using Application.DTOs.Services;
using Application.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class S3StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<S3StorageService> _logger;
        private readonly string _productStorage;

        public S3StorageService(IAmazonS3 s3Client, IConfiguration config, ILogger<S3StorageService> logger)
        {
            _s3Client = s3Client;
            _logger = logger;
            _productStorage = config["Storage:product_storage"] ?? "";
        }
        private string GetBucketName(StorageType type)
        {
            return type == StorageType.product ? _productStorage : _productStorage;
        }
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, StorageType type)
        {
            string targetBucket = GetBucketName(type);
            var request = new PutObjectRequest
            {
                BucketName = targetBucket,
                Key = fileName,
                InputStream = fileStream,
                ContentType = contentType
            };

            await _s3Client.PutObjectAsync(request);
            return $"{_s3Client.Config.ServiceURL}{_productStorage}/{fileName}";
        }
        public async Task DeleteFileAsync(string fileName, StorageType type)
        {
            string targetBucket = GetBucketName(type);
            await _s3Client.DeleteObjectAsync(targetBucket, fileName);
        }
        public async Task<bool> FileExistsAsync(string fileName, StorageType type)
        {
            try
            {
                string targetBucket = GetBucketName(type);
                await _s3Client.GetObjectMetadataAsync(targetBucket, fileName);
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return false;
                }
                throw;
            }
        }
        public async Task<Stream> GetFileStreamAsync(string fileName, StorageType type)
        {
            try
            {
                string targetBucket = GetBucketName(type);
                var request = new GetObjectRequest
                {
                    BucketName = targetBucket,
                    Key = fileName
                };

                var response = await _s3Client.GetObjectAsync(request);
                return response.ResponseStream;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Failed to get file from S3: {FileName}", fileName);
                throw new Exception("Không thể tải file. Vui lòng thử lại sau.");
            }
        }
    }
}

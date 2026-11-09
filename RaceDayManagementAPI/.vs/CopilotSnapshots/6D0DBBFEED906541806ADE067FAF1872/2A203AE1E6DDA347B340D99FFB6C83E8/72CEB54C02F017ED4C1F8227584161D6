using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDayManagementAPI.Services
{
    public class BlobService : IBlobService
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<BlobService> _logger;

        public BlobService(IConfiguration config, IWebHostEnvironment env, ILogger<BlobService> logger)
        {
            _config = config;
            _env = env;
            _logger = logger;
        }

        private (string? ConnectionString, string ContainerName) GetBlobConfig()
        {
            var connectionString = _config["AzureBlobStorage:ConnectionString"]
                ?? _config["JwtSettings:AzureBlobStorage:ConnectionString"];
            var containerName = _config["AzureBlobStorage:ContainerName"]
                ?? _config["JwtSettings:AzureBlobStorage:ContainerName"]
                ?? "event-banners";

            return (connectionString, containerName);
        }

        private static bool IsConfiguredConnectionString(string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return false;

            // Detect unconfigured placeholder keys such as AccountKey=...
            if (connectionString.Contains("AccountKey=..."))
                return false;

            return true;
        }

        public async Task<string> UploadAsync(IFormFile file, string fileName)
        {
            var (connectionString, containerName) = GetBlobConfig();

            if (IsConfiguredConnectionString(connectionString))
            {
                try
                {
                    var serviceClient = new BlobServiceClient(connectionString);
                    var container = serviceClient.GetBlobContainerClient(containerName);
                    await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

                    var blobClient = container.GetBlobClient(fileName);
                    using var stream = file.OpenReadStream();
                    await blobClient.UploadAsync(stream, new BlobHttpHeaders
                    {
                        ContentType = file.ContentType ?? "application/octet-stream"
                    });

                    return blobClient.Uri.ToString();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Azure Blob Storage upload failed for {FileName}. Falling back to local storage.", fileName);
                }
            }
            else
            {
                _logger.LogInformation("Azure Blob Storage connection string is a placeholder or not set. Saving banner {FileName} locally.", fileName);
            }

            // Local fallback so banner upload works even without live Azure credentials
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadsFolder = Path.Combine(webRoot, "uploads", "banners");
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/banners/{fileName}";
        }

        public async Task DeleteAsync(string fileName)
        {
            var (connectionString, containerName) = GetBlobConfig();

            if (IsConfiguredConnectionString(connectionString))
            {
                try
                {
                    var serviceClient = new BlobServiceClient(connectionString);
                    var container = serviceClient.GetBlobContainerClient(containerName);
                    var blobClient = container.GetBlobClient(fileName);
                    await blobClient.DeleteIfExistsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete blob {FileName} from Azure Blob Storage.", fileName);
                }
            }

            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var localFile = Path.Combine(webRoot, "uploads", "banners", fileName);
            if (File.Exists(localFile))
            {
                File.Delete(localFile);
            }
        }
    }
}

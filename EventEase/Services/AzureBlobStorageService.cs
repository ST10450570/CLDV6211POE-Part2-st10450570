using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace EventEase.Services
{
    public class BlobStorageService : IImageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public BlobStorageService(IConfiguration configuration)
        {
            // Correctly access the connection string from the AzureBlobStorage section
            string connectionString = configuration.GetSection("AzureBlobStorage")["ConnectionString"];
            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerName = configuration.GetSection("AzureBlobStorage")["ContainerName"];

            // Create the container if it doesn't exist
            CreateContainerIfNotExistsAsync().Wait();
        }

        private async Task CreateContainerIfNotExistsAsync()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return null;

            // Get a reference to the container
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            // Create a unique file name
            var fileName = $"{folderName}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blobClient = containerClient.GetBlobClient(fileName);

            // Upload the file
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            // Return the URI of the uploaded blob
            return blobClient.Uri.ToString();
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            try
            {
                // Extract the blob name from the URL
                var uri = new Uri(imageUrl);
                var blobName = uri.Segments[uri.Segments.Length - 1];
                var folderName = uri.Segments[uri.Segments.Length - 2].TrimEnd('/');
                var fullBlobName = $"{folderName}/{blobName}";

                // Get a reference to the blob
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(fullBlobName);

                // Delete the blob
                await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete blob");
            }

        }

        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
        {
            _logger = logger;

            string connectionString = configuration.GetSection("AzureBlobStorage")["ConnectionString"];
            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerName = configuration.GetSection("AzureBlobStorage")["ContainerName"];

            CreateContainerIfNotExistsAsync().Wait();
        }

    }
}


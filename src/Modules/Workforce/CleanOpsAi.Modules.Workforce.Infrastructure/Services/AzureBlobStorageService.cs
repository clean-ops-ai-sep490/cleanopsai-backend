using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Services
{
    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureBlob:ConnectionString"];

            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName)
        {
            var container = _blobServiceClient.GetBlobContainerClient(containerName);

            await container.CreateIfNotExistsAsync();

            // giữ folder avatars
            var extension = Path.GetExtension(fileName);
            var folder = Path.GetDirectoryName(fileName);

            var newFileName = $"{Guid.NewGuid()}{extension}";

            if (!string.IsNullOrEmpty(folder))
                newFileName = $"{folder}/{newFileName}";

            var blobClient = container.GetBlobClient(newFileName);

            // detect content type
            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(newFileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var headers = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(fileStream, new BlobUploadOptions
            {
                HttpHeaders = headers
            });

            return GenerateSasUrl(newFileName, containerName);
        }

        public string GenerateSasUrl(string fileName, string containerName)
        {
            var container = _blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = container.GetBlobClient(fileName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = fileName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }
    }
}

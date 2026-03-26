using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Services
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

            var extension = Path.GetExtension(fileName);
            var folder = Path.GetDirectoryName(fileName);
            var newFileName = $"{Guid.NewGuid()}{extension}";

            if (!string.IsNullOrEmpty(folder))
                newFileName = $"{folder}/{newFileName}";

            var blobClient = container.GetBlobClient(newFileName);

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(newFileName, out var contentType))
                contentType = "application/octet-stream";

            await blobClient.UploadAsync(fileStream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            });

            // Luu SAS URL voi expiry dai (1 nam) thang vao DB
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
                // Tang expiry len 1 nam thay vi 30 phut
                ExpiresOn = DateTimeOffset.UtcNow.AddYears(1)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }
    }
}

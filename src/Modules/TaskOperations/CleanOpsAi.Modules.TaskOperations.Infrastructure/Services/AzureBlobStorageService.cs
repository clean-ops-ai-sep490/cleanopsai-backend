using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration; 

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

        // luu danh sach file
        public async Task<List<string>> UploadFilesAsync(
            IEnumerable<(Stream Stream, string FileName)> files,
            string containerName,
            CancellationToken ct = default)
        {
            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync(cancellationToken: ct);

            // Upload song song các file
            var uploadTasks = files.Select(async file =>
            {
                var blobName = BuildBlobName(file.FileName);
                var blobClient = container.GetBlobClient(blobName);

                await blobClient.UploadAsync(file.Stream, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = ResolveContentType(blobName) }
                }, ct);

                return GenerateSasUrl(blobName, containerName);
            });

            var urls = await Task.WhenAll(uploadTasks);
            return urls.ToList();
        }

        /// Tạo tên blob unique, giữ nguyên folder prefix nếu có
        private static string BuildBlobName(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            var folder = Path.GetDirectoryName(fileName)?.Replace("\\", "/");
            var uniqueName = $"{Guid.NewGuid()}{extension}";

            return string.IsNullOrEmpty(folder)
                ? uniqueName
                : $"{folder}/{uniqueName}";
        }

        private static string ResolveContentType(string blobName)
        {
            var provider = new FileExtensionContentTypeProvider();
            return provider.TryGetContentType(blobName, out var contentType)
                ? contentType
                : "application/octet-stream";
        }

    }
}

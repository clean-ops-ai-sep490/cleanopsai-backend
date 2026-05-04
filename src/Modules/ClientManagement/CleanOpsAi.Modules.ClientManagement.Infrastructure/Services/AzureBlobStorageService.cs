using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Services
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

            var newFileName = Guid.NewGuid() + Path.GetExtension(fileName);

            var blobClient = container.GetBlobClient(newFileName);

            await blobClient.UploadAsync(fileStream, overwrite: true);

            return blobClient.Uri.ToString();

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

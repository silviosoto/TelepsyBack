using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using System;

using Microsoft.Extensions.Configuration;
using TelePsy.BLL.Interfaces;

namespace TelePsy.BLL.Services
{
    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly string _connectionString;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AzureStorage") ?? string.Empty;
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string containerName)
        {
            var container = new BlobContainerClient(_connectionString, containerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.None);

            var blob = container.GetBlobClient(fileName);
            await blob.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = GetContentType(fileName) });

            return fileName;
        }

        private string GetSasUrl(BlobClient blob)
        {
            if (blob.CanGenerateSasUri)
            {
                var sasUri = blob.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(24));
                return sasUri.ToString();
            }
            return blob.Uri.ToString();
        }

        public async Task DeleteFileAsync(string fileUrl, string containerName)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            var container = new BlobContainerClient(_connectionString, containerName);
            var blobName = Path.GetFileName(fileUrl);
            var blob = container.GetBlobClient(blobName);

            await blob.DeleteIfExistsAsync();
        }

        public Task<string> GetFileUrlAsync(string fileName, string containerName)
        {
            var container = new BlobContainerClient(_connectionString, containerName);
            var blob = container.GetBlobClient(fileName);
            return Task.FromResult(GetSasUrl(blob));
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream",
            };
        }
    }
}

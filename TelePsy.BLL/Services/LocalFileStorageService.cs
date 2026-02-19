using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using TelePsy.BLL.Interfaces;

namespace TelePsy.BLL.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalFileStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string containerName)
        {
            var folder = Path.Combine(_env.WebRootPath, containerName);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var filePath = Path.Combine(folder, fileName);
            using (var destinationStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(destinationStream);
            }

            var request = _httpContextAccessor.HttpContext.Request;
            var host = request.Host.ToUriComponent();
            var scheme = request.Scheme;
            
            return $"{scheme}://{host}/{containerName}/{fileName}";
        }

        public Task DeleteFileAsync(string fileUrl, string containerName)
        {
            if (string.IsNullOrEmpty(fileUrl)) return Task.CompletedTask;

            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_env.WebRootPath, containerName, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }

        public Task<string> GetFileUrlAsync(string fileName, string containerName)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var host = request.Host.ToUriComponent();
            var scheme = request.Scheme;
            
            return Task.FromResult($"{scheme}://{host}/{containerName}/{fileName}");
        }
    }
}

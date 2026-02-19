using System.IO;
using System.Threading.Tasks;

namespace TelePsy.BLL.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(Stream fileStream, string fileName, string containerName);
        Task DeleteFileAsync(string fileUrl, string containerName);
        Task<string> GetFileUrlAsync(string fileName, string containerName);
    }
}

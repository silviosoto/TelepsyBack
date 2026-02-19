using System.Threading.Tasks;

namespace TelePsy.BLL.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(string userId, string action, string entityName, string entityId, string details);
    }
}

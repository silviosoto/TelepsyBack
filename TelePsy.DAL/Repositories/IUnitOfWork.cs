using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;

namespace TelePsy.DAL.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : class;
        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        IExecutionStrategy CreateExecutionStrategy();
    }
}

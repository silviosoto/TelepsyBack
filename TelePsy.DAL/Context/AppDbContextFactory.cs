using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TelePsy.DAL.Context
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=TelePsyDb;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}

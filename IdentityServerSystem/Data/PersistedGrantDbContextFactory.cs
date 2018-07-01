using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace IdentityServerSystem.Data
{
    public class PersistedGrantDbContextFactory : IDesignTimeDbContextFactory<PersistedGrantDbContext>
    {


        public PersistedGrantDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PersistedGrantDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=aspnet-IdentityServerSystemOpenID2;Trusted_Connection=True;MultipleActiveResultSets=true", b => b.MigrationsAssembly("IdentityServerSystem"));
            var configStoreOptions = new OperationalStoreOptions();
            return new PersistedGrantDbContext(optionsBuilder.Options, configStoreOptions);
        }
    }
}

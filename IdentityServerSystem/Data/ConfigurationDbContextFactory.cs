using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.WebSockets.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace IdentityServerSystem.Data
{
    public class ConfigurationDbContextFactory : IDesignTimeDbContextFactory<ConfigurationDbContext>
    {
        ConfigurationDbContext IDesignTimeDbContextFactory<ConfigurationDbContext>.CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ConfigurationDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=aspnet-IdentityServerSystemOpenID2;Trusted_Connection=True;MultipleActiveResultSets=true",
                 b => b.MigrationsAssembly("IdentityServerSystem"));
            var configStoreOptions = new ConfigurationStoreOptions();
            return new ConfigurationDbContext(optionsBuilder.Options, configStoreOptions);
        }
              
    }
}





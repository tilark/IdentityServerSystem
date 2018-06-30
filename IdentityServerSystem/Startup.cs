using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IdentityServerSystem.Data;
using IdentityServerSystem.Models;
using IdentityServerSystem.Services;
using System.Reflection;
using IdentityServer4.Validation;
using IdentityServer4.Services;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using IdentityServerSystem.Models.GetUserInfoFromWebApiViewModels;
using IdentityServer4.EntityFramework.Interfaces;

namespace IdentityServerSystem
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        //public Startup(IHostingEnvironment env)
        //{
        //    var builder = new ConfigurationBuilder()
        //        .SetBasePath(env.ContentRootPath)
        //        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        //        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

        //    if (env.IsDevelopment())
        //    {
        //        // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
        //        builder.AddUserSecrets<Startup>();
        //    }

        //    builder.AddEnvironmentVariables();
        //    Configuration = builder.Build();
        //}

        //public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Add In Memory
            services.AddMemoryCache();
            // Add framework services.
            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseMySql(Configuration.GetConnectionString("mysqlApplicationDBConnection")));
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            //var connectionString = Configuration.GetConnectionString("mysqlIdentityServerDBConnection");
            var connectionString = Configuration.GetConnectionString("IdentityServerConnection");

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            
            //services.AddIdentityServer()
            //   .AddDeveloperSigningCredential("identityserver.rsa")
            //   .AddAspNetIdentity<ApplicationUser>()
            //   .AddConfigurationStore(builder =>
            //       builder.UseSqlServer(connectionString, options =>
            //   options.MigrationsAssembly(migrationsAssembly)))
            //   .AddOperationalStore(builder =>
            //       builder.UseSqlServer(connectionString, options =>
            //   options.MigrationsAssembly(migrationsAssembly)))
            //   ;
            //services.AddIdentityServer()
            //    .AddDeveloperSigningCredential("identityserver.rsa")
            //    .AddAspNetIdentity<ApplicationUser>()
            //    .AddConfigurationStore(builder =>
            //        builder.UseMySql(connectionString, options =>
            //    options.MigrationsAssembly(migrationsAssembly)))
            //    .AddOperationalStore(builder =>
            //        builder.UseMySql(connectionString, options =>
            //    options.MigrationsAssembly(migrationsAssembly)))
            //    ;

            services.AddMvc();

            services.AddIdentityServer()
               .AddDeveloperSigningCredential(true, "identityserver.rsa")
               .AddAspNetIdentity<ApplicationUser>()
               .AddConfigurationStore(options => {
                   options.ConfigureDbContext = builder =>
                    builder.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
               })
               .AddOperationalStore(options =>
               {
                   options.ConfigureDbContext = builder =>
                    builder.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                   options.EnableTokenCleanup = true;
                   options.TokenCleanupInterval = 30;
               });
            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            //services.AddTransient<IConfigurationDbContext, ConfigurationDbContext>();
            // Configure Identity
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 4;
                //非字符
                options.Password.RequireNonAlphanumeric = false;
                //大写
                options.Password.RequireUppercase = false;
                //小写
                options.Password.RequireLowercase = false;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;
              
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Administrator", policy => policy.RequireClaim("Administrator", "Administrator"));
                options.AddPolicy("AdminUser", policy => policy.RequireClaim("Administrator", "Administrator", "AdminUser"));

            });
            //services.AddTransient<IProfileService, ProfileService>();
            //services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();
            //services.AddScoped<DropDownListService>();
            services.AddScoped<ConfigDbContextDropDownListService>();

            //添加Configuration
            services.Configure<HumanResourceIdentityOption>(Configuration.GetSection("HumanResourceSystemIdentity"));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // this will do the initial DB population
            InitializeDatabase(app);

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            //app.UseIdentity();
            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715
            // Adds IdentityServer
            app.UseIdentityServer();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var persisterGrantDbContext = serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database;
                
                    //persisterGrantDbContext.Migrate();

               
                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                
                    //context.Database.Migrate();
                              
               

                if (!context.Clients.Any())
                {
                    bool canSaveChange = false;
                    foreach (var client in Config.GetClients())
                    {
                        //如果数据库中已经存在，则不添加
                        var query = context.Clients.FirstOrDefault(a => a.ClientName == client.ClientName);
                        if(query == null)
                        {
                            canSaveChange = true;
                            context.Clients.Add(client.ToEntity());
                        }
                    }
                    if(canSaveChange)
                        context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    bool canSaveChange = false;

                    foreach (var resource in Config.GetIdentityResources())
                    {
                        var query = context.IdentityResources.FirstOrDefault(a => a.Name == resource.Name);
                        if(query == null)
                        {
                            canSaveChange = true;
                            context.IdentityResources.Add(resource.ToEntity());

                        }
                    }
                    if(canSaveChange)
                        context.SaveChanges();
                }   

                if (!context.ApiResources.Any())
                {
                    bool canSaveChange = false;
                    foreach (var resource in Config.GetApiResources())
                    {
                        var query = context.ApiResources.FirstOrDefault(a => a.Name == resource.Name);
                        if(query == null)
                        {
                            context.ApiResources.Add(resource.ToEntity());
                            canSaveChange = true;
                        }
                    }
                    if(canSaveChange)
                        context.SaveChanges();
                }

                //初始化创建Administrator用户
                // First apply pendding migrations if exist
                //serviceScope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();

                // Then call seeder method
                var userManager = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                serviceScope.ServiceProvider.GetService<ApplicationDbContext>().EnsureSeedData(userManager);
            }
        }
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using System.Linq;
using Rsk.AspNetCore.Authentication.Saml2p;
using IdSrv.Data.Context;
using IdSrv.Data;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using IdSrv.Quickstart;
using Rsk.AspNetCore.Authentication.Saml2p.Factories;
using IdentityServer4.Saml.Configuration;
using IdSrv.Data.Models;
using System.Threading.Tasks;
using IdentityServer4.Saml.Generators.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Http;
using Rsk.AspNetCore.Authentication.Saml2p.Cookies;
using IdentityServer4.Saml.Validation.Interfaces;
using IdentityServer4.Saml.Services;
using IdentityServer4.Saml.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Logging;
using IdentityModel;
using Swashbuckle.AspNetCore.Swagger;
using IdentityServer4.Saml.Caching;

namespace IdSrv
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            const string connectionString = @"Data Source=.\SQLExpress;database=IdsDatabase;trusted_connection=yes;";
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            });

            services.AddDbContext<IS4DbContext>(opt => opt.UseSqlServer(connectionString));

            services.AddHttpContextAccessor();
            services
                .AddIdentityServer()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));

                    options.EnableTokenCleanup = true;
                })
                .AddProfileService<ProfileService>()
                .AddSigningCredential(new X509Certificate2("idsrv3test.pfx", "idsrv3test"))
                .AddSamlPlugin(options => {
                    options.Licensee = "Demo";
                    options.LicenseKey = "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsImF1dGgiOiJERU1PIiwiZXhwIjoiMjAxOS0wOS0xMlQwMTowMDowMS4wODkwNTkzKzAxOjAwIiwiaWF0IjoiMjAxOS0wOC0xM1QwMDowMDowMS4wMDAwNTkzIiwib3JnIjoiREVNTyIsImF1ZCI6Mn0=.aIIw1/atlaiBci5zSxPIhdinI7tEmBZYK3Xuhe19a5twDeSzRm/i57gWpMMu/2x8O40nfAi9yk2Gbpadzv8RpOxaF5+OnnIcg4yevKLipJKZJ9bBDDC78Yl/nkhM6N65e18haUmgO/l8EYeRpYNTBE7dNGK2wZJAyqDpWpBYQ8tbCvfjchj5GGqudqFT/YLNuHn1y0Ps7mP4rh4V5G7TyIaY9QfW3ZMGJGtOQyjvTzlc02IZrw6idhRpNNazMqZAFnUKUV+bR//jyx1uBkz1H+hF0eslQt9Wt9yse3JAZi/D4oUTFyPOfzzhGkW2ceftX8tmYEWE/lPjQ3+xonKX+XNxJEXjib6d6GkTJpALpBniXS6XIKDnDV2h0Bs14lMA+2ddEDBMWVid4ubdUBZBkQLKWfZkPACUd6aBfx001v3QE2YQ9yxzdq0EAWnvEV5EE3NR4sLaNNuYxxMSoBs8ZRovhcT7Yo+o6skNu1cVvGVjOl+YMURdQgc7dPU+NxNG4oekEyW+i9N3RidvZeM1xz8dQipBMCMrrOymLV5hT32N9Y/XX4yC26zbuc5aYoaQKNqF7ZVfbrOa2H4G/Jr3pYj3EWRWfy2xlLDbUSzjcJ4ut2OZFHs5ql2Ut9y/LQ8yB3Impur6h8TpwbziWza/z2j2Gmj3rwKbN0vO2zYQJv4=";
                })
                .AddInMemoryServiceProviders(Config.GetServiceProviders());
            IdentityModelEventSource.ShowPII = true;
            services.AddTransient<Repository>();
            services.AddTransient<OpenIdConnectPostConfigureOptions>();
            services.AddTransient<TenantResolver>();
            services.AddTransient<AuthSchemeProvider>();

            var builder = services.AddAuthentication(opt => opt.DefaultChallengeScheme = "oidc");

            LoadSamlDependencies(builder);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddFile("Logs/{Date}-idsrv-log.txt");
            loggerFactory.AddDebug();

            InitializeDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseIdentityServer()
                .UseIdentityServerSamlPlugin();

            app.UseAuthentication();
            app.UseStaticFiles();

            app.UseMvcWithDefaultRoute();
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var configurationContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                configurationContext.Database.Migrate();

                var authSchemeProvider = serviceScope.ServiceProvider.GetRequiredService<AuthSchemeProvider>();

                //this method will be executed only if we already have config file for IDS4
                //if not put it in comments
                PopulateDBWithConfig(configurationContext);

                //----------------------***************--------------------------
                //Uncomment after migrating IDS4DbContext
                GetAllSchemes(authSchemeProvider).GetAwaiter().GetResult();
                //----------------------***************--------------------------
            }

        }

        private async Task GetAllSchemes(AuthSchemeProvider authSchemeProvider)
        {
            await authSchemeProvider.LoadAllSchemes();
        }

        private void LoadSamlDependencies(AuthenticationBuilder builder)
        {
            builder.Services.AddMemoryCache();
            builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            builder.Services.TryAddScoped<ISamlFactory<IServiceProviderMetadataGenerator>, ServiceProviderMetadataGeneratorFactory>();
            builder.Services.TryAddScoped<ISamlFactory<ISaml2SingleSignOnRequestGenerator>, Saml2SingleSignOnRequestGeneratorFactory>();
            builder.Services.TryAddScoped<ISamlFactory<ISaml2SingleLogoutRequestGenerator>, Saml2SingleLogoutRequestGeneratorFactory>();
            builder.Services.TryAddScoped<ISamlFactory<ISaml2SingleSignOnResponseValidator>, Saml2SingleSignOnResponseValidatorFactory>();
            builder.Services.TryAddScoped<IReplayDetectionService, ReplayDetectionService>();
            builder.Services.TryAddScoped<IReplayCache, DistributedReplayCache>();

            builder.Services.TryAddScoped<ISamlBindingService, SamlBindingService>();
            builder.Services.TryAddScoped<ISamlSigningService, SamlSigningService>();
            builder.Services.TryAddScoped<IDateTimeService, SystemClockDateTimeService>();
            builder.Services.TryAddScoped<ISamlTimeComparer, SamlTimeComparer>();
            builder.Services.TryAddScoped<ISamlCorrelationStore, CookieCorrelationStore>();
        }

        private void PopulateDBWithConfig(ConfigurationDbContext context)
        {
            if (!context.Clients.Any())
            {
                foreach (var client in Config.GetClients())
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var resource in Config.GetIdentityResources())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var resource in Config.GetApiResources())
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
        }
    }
}

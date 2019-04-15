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
            services.AddTransient<AuthSchemeProvider>();

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
                    options.LicenseKey = "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsImF1dGgiOiJERU1PIiwiZXhwIjoiMjAxOS0wNS0xMVQwMTowMDowMC45Njc1MTI5KzAxOjAwIiwiaWF0IjoiMjAxOS0wNC0xMVQwMDowMDowMC4wMDA1MTI5Iiwib3JnIjoiREVNTyIsImF1ZCI6Mn0=.PKaOIARp3KB5GYWCoBXkJHOSqpGg2BcPpQX9/N/4SJ9rFyI3CLGILF/qrQzWBBUdTTKxMGU4LaPToCQE3XAF1M4Ikh8QCYNMsS2o5OlfO2+sqmVvI6Y8ucjMgwPnEigFW+q1+mZbWHlqPto0OsHhSjX8PgZb+g8nsWbGb5MLSAaM+8bLgcghizj3xZctr6QyOI0a9p9VThzPSNi4hKEyPBZ/EOjt7Qxh2R4TVsY9TnbeTIRT+P9yGXGQoJqmIIqVhlMu7v6Qe0hV1orgLKonJpqQVRsYUK/rl9ygyMV7lfB3KQ4k3EwqsUFF9OclMF+DZBNa1YOfqBmYnVebQWmpvkqxh/RiEiRrh+ERoGDNrOBgPbPlNj80dxy37rkqZSFyNg6su3F7v3ZyjFyS9kVha0HsnhkvN8Kz14myzokxwiBe2BVDy7ErzXajhP3q8a04SP6qL22mO9uBwAppHPD7UOU88+CC3GHVD5NmSjzMwN2sNzgExjOQ4dFDjvfcz9byilMUPCW1o3vRcIVA7CJx7F28ZYQFw/LuBlTPcZ9LlkWq1LptZR1KeusKX5vcz5QvWj7F+uO1fA2uwrUSdghGGkHbfLrh6OJmUFUD8HGm0N83ydLQKaEMMJgeA4T3ox19zycws7RdrZ6uLX2hTySCZ5xjf6eUu97QDxxd/LEnQvU=";
                })
                .AddInMemoryServiceProviders(Config.GetServiceProviders());
            IdentityModelEventSource.ShowPII = true;
            services.AddTransient<Repository>();
            services.AddTransient<OpenIdConnectPostConfigureOptions>();
            services.AddTransient<TenantResolver>();

            var builder = services.AddAuthentication(opt => opt.DefaultChallengeScheme = "oidc")
                .AddOpenIdConnect("test", "test", opt => 
                {
                    opt.Authority = "https://login.microsoftonline.com/9a433611-0c81-4f7b-abae-891364ddda17/";
                    opt.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    opt.SignOutScheme = IdentityServerConstants.SignoutScheme;
                    opt.ClientId = "a0760163-2bac-445b-9052-f34de309bb64";
                    opt.RequireHttpsMetadata = false;
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = JwtClaimTypes.Subject,
                        RoleClaimType = JwtClaimTypes.Role,
                    };
                    
                });

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
                //PopulateDBWithConfig(configurationContext);

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

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

namespace IdSrv
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            const string connectionString = @"Data Source=.\SQLExpress;database=IdentityServer4.Quickstart.EntityFramework;trusted_connection=yes;";
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddMvc();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddHttpContextAccessor();
            services
                .AddIdentityServer()
                .AddTestUsers(Config.GetUsers())
                // this adds the config data from DB (clients, resources)
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                })
                //.AddInMemoryIdentityResources(Config.GetIdentityResources())
                //.AddInMemoryApiResources(Config.GetApiResources())
                //.AddInMemoryClients(Config.GetClients())
                .AddProfileService<ProfileService>()
                .AddSigningCredential(new X509Certificate2("idsrv3test.pfx", "idsrv3test"))
                .AddSamlPlugin(options => {
                    options.Licensee = "Demo";
                    options.LicenseKey = "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsImF1dGgiOiJERU1PIiwiZXhwIjoiMjAxOS0wNS0xMVQwMTowMDowMC45Njc1MTI5KzAxOjAwIiwiaWF0IjoiMjAxOS0wNC0xMVQwMDowMDowMC4wMDA1MTI5Iiwib3JnIjoiREVNTyIsImF1ZCI6Mn0=.PKaOIARp3KB5GYWCoBXkJHOSqpGg2BcPpQX9/N/4SJ9rFyI3CLGILF/qrQzWBBUdTTKxMGU4LaPToCQE3XAF1M4Ikh8QCYNMsS2o5OlfO2+sqmVvI6Y8ucjMgwPnEigFW+q1+mZbWHlqPto0OsHhSjX8PgZb+g8nsWbGb5MLSAaM+8bLgcghizj3xZctr6QyOI0a9p9VThzPSNi4hKEyPBZ/EOjt7Qxh2R4TVsY9TnbeTIRT+P9yGXGQoJqmIIqVhlMu7v6Qe0hV1orgLKonJpqQVRsYUK/rl9ygyMV7lfB3KQ4k3EwqsUFF9OclMF+DZBNa1YOfqBmYnVebQWmpvkqxh/RiEiRrh+ERoGDNrOBgPbPlNj80dxy37rkqZSFyNg6su3F7v3ZyjFyS9kVha0HsnhkvN8Kz14myzokxwiBe2BVDy7ErzXajhP3q8a04SP6qL22mO9uBwAppHPD7UOU88+CC3GHVD5NmSjzMwN2sNzgExjOQ4dFDjvfcz9byilMUPCW1o3vRcIVA7CJx7F28ZYQFw/LuBlTPcZ9LlkWq1LptZR1KeusKX5vcz5QvWj7F+uO1fA2uwrUSdghGGkHbfLrh6OJmUFUD8HGm0N83ydLQKaEMMJgeA4T3ox19zycws7RdrZ6uLX2hTySCZ5xjf6eUu97QDxxd/LEnQvU=";
                })
                
                .AddInMemoryServiceProviders(Config.GetServiceProviders());
            services.AddAuthentication(opt => opt.DefaultChallengeScheme = "adfs")
                .AddOpenIdConnect("AAD", "Azure Active Directory", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                    options.Authority = "https://login.microsoftonline.com/common";
                    options.ClientId = "f6a6b204-ff96-4013-8adb-7b1ef0bdda2a";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false
                    };
                    options.GetClaimsFromUserInfoEndpoint = true;
                }).AddSaml2p("adfs", "AD authentication", options =>
                {
                    options.Licensee = "Demo";
                    options.LicenseKey = "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsImF1dGgiOiJERU1PIiwiZXhwIjoiMjAxOS0wNS0xMVQwMTowMDowMC45Njc1MTI5KzAxOjAwIiwiaWF0IjoiMjAxOS0wNC0xMVQwMDowMDowMC4wMDA1MTI5Iiwib3JnIjoiREVNTyIsImF1ZCI6Mn0=.PKaOIARp3KB5GYWCoBXkJHOSqpGg2BcPpQX9/N/4SJ9rFyI3CLGILF/qrQzWBBUdTTKxMGU4LaPToCQE3XAF1M4Ikh8QCYNMsS2o5OlfO2+sqmVvI6Y8ucjMgwPnEigFW+q1+mZbWHlqPto0OsHhSjX8PgZb+g8nsWbGb5MLSAaM+8bLgcghizj3xZctr6QyOI0a9p9VThzPSNi4hKEyPBZ/EOjt7Qxh2R4TVsY9TnbeTIRT+P9yGXGQoJqmIIqVhlMu7v6Qe0hV1orgLKonJpqQVRsYUK/rl9ygyMV7lfB3KQ4k3EwqsUFF9OclMF+DZBNa1YOfqBmYnVebQWmpvkqxh/RiEiRrh+ERoGDNrOBgPbPlNj80dxy37rkqZSFyNg6su3F7v3ZyjFyS9kVha0HsnhkvN8Kz14myzokxwiBe2BVDy7ErzXajhP3q8a04SP6qL22mO9uBwAppHPD7UOU88+CC3GHVD5NmSjzMwN2sNzgExjOQ4dFDjvfcz9byilMUPCW1o3vRcIVA7CJx7F28ZYQFw/LuBlTPcZ9LlkWq1LptZR1KeusKX5vcz5QvWj7F+uO1fA2uwrUSdghGGkHbfLrh6OJmUFUD8HGm0N83ydLQKaEMMJgeA4T3ox19zycws7RdrZ6uLX2hTySCZ5xjf6eUu97QDxxd/LEnQvU=";
                    options.IdentityProviderOptions = new IdpOptions
                    {
                        EntityId = "https://adfs.it-labs.io/adfs/services/trust",
                        SigningCertificate = new X509Certificate2("adfs-signing.cer"),
                        SingleSignOnEndpoint = new SamlEndpoint("https://adfs.it-labs.io/adfs/ls/", SamlBindingTypes.HttpRedirect),
                        SingleLogoutEndpoint = new SamlEndpoint("https://adfs.it-labs.io/adfs/ls/", SamlBindingTypes.HttpRedirect),
                    };
                    options.ServiceProviderOptions = new SpOptions
                    {
                        EntityId = "https://localhost:44374/saml",
                        MetadataPath = "/saml/metadata",
                        SignAuthenticationRequests = true,
                        SigningCertificate = new X509Certificate2("testclient.pfx", "test")
                    };

                    options.SaveTokens = true;
                    options.NameIdClaimType = "sub";
                    options.CallbackPath = "/signin-saml";
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.TimeComparisonTolerance = 15;
                });
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

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();

                //this method will be executed only if we already have config file for IDS4
                //if not put it in comments
                PopulateDBWithConfig(context);
            }
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

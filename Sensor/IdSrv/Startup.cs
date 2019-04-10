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
                    options.LicenseKey = "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsImF1dGgiOiJERU1PIiwiZXhwIjoiMjAxOS0wNC0xMVQwMDowMDowMCIsImlhdCI6IjIwMTktMDMtMTFUMTE6NDk6NTguMDAwMDYwMiIsIm9yZyI6IkRFTU8iLCJhdWQiOjJ9.MoX1OmC40bJ/nlrdYaMdAW365sBQMOy4wGxe+50Sjrg2dFg9ZSM6GwGN0rB/cm43IiEDLaX/0XWdOA0/jQ/B3NofC7914GOSepKrYFFXE0NukdAQoq50lk5iUhfQe9jJD12+djGTgHHMBOPncWHvbLfxz2QJ8z59vGPL/Qh/gUTTrg3L5KYvg9tiVvOPDl092vKGwg8aTcQa/n61Csxt2dhCsJ6xYFK9HTYI1l2Lj0l1lFC6m4RtQ4Ip9CWQx8GBMMzz2tNzeh6QHw26DdFEABu4RFt3p90QKzp/h7R/ISCkC9AyVrAW688PYZ5hrhs8eD72Mm4OPn0Og0XdnDdJPSOJBVuTTY7+OCjNNKq69/PQjyhdjpIH1K8rjvfrq17G/k8SE5QDaGyZqui/wFbyFz6L4aLuxz0f7sydf69V2pZox6/Z8PkJezHGwxVbn8+UOs0e+7OY1nqWW41mgq1B+pDZX5xO+wAOkq4jXfqnlESZA0D6uIXYU2jRf2dB5VpgyZB0NrmyM2egpUTDxpEjkfOB9kYc53CWs2lGI48DlVzMaZ2jIWcqrqNcLSm8tPav152ftf1SJZewNIx+bdnb+kmk1IEe3+9APRYCfpiI3yk7eeaJcJmB6LXsGs1ATz7GhauAAYRW7I2LpzepzQ/JapG+KFiwfNAX2Q+Fk8rHpeI=";
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
                    options.LicenseKey = "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsImF1dGgiOiJERU1PIiwiZXhwIjoiMjAxOS0wNC0xMVQwMDowMDowMCIsImlhdCI6IjIwMTktMDMtMTFUMTE6NDk6NTguMDAwMDYwMiIsIm9yZyI6IkRFTU8iLCJhdWQiOjJ9.MoX1OmC40bJ/nlrdYaMdAW365sBQMOy4wGxe+50Sjrg2dFg9ZSM6GwGN0rB/cm43IiEDLaX/0XWdOA0/jQ/B3NofC7914GOSepKrYFFXE0NukdAQoq50lk5iUhfQe9jJD12+djGTgHHMBOPncWHvbLfxz2QJ8z59vGPL/Qh/gUTTrg3L5KYvg9tiVvOPDl092vKGwg8aTcQa/n61Csxt2dhCsJ6xYFK9HTYI1l2Lj0l1lFC6m4RtQ4Ip9CWQx8GBMMzz2tNzeh6QHw26DdFEABu4RFt3p90QKzp/h7R/ISCkC9AyVrAW688PYZ5hrhs8eD72Mm4OPn0Og0XdnDdJPSOJBVuTTY7+OCjNNKq69/PQjyhdjpIH1K8rjvfrq17G/k8SE5QDaGyZqui/wFbyFz6L4aLuxz0f7sydf69V2pZox6/Z8PkJezHGwxVbn8+UOs0e+7OY1nqWW41mgq1B+pDZX5xO+wAOkq4jXfqnlESZA0D6uIXYU2jRf2dB5VpgyZB0NrmyM2egpUTDxpEjkfOB9kYc53CWs2lGI48DlVzMaZ2jIWcqrqNcLSm8tPav152ftf1SJZewNIx+bdnb+kmk1IEe3+9APRYCfpiI3yk7eeaJcJmB6LXsGs1ATz7GhauAAYRW7I2LpzepzQ/JapG+KFiwfNAX2Q+Fk8rHpeI=";
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

// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


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
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Rsk.AspNetCore.Authentication.Saml2p.Factories;
using IdentityServer4.Saml.Configuration;
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
using AutoMapper.Configuration;
using IdentityServerAspNetIdentity.Data;
using Microsoft.AspNetCore.Identity;
using System;
using IdentityServerAspNetIdentity.Quickstart;
using IdentityServerAspNetIdentity.Data.Models;
using System.Security.Claims;
using AutoMapper;

namespace IdentityServerAspNetIdentity
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var connectionString = @"Data Source=.\SQLExpress;database=IDS;trusted_connection=yes;";
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddHttpContextAccessor();

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

           services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
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
                .AddAspNetIdentity<ApplicationUser>()
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
            services.AddTransient<AuthSchemeProvider>();

            var builder = services.AddAuthentication(opt => opt.DefaultChallengeScheme = "oidc");

            LoadSamlDependencies(builder);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            InitializeDatabase(app);

            
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
               EnsureSeedData(serviceScope.ServiceProvider);

            }

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

        public void EnsureSeedData(IServiceProvider provider)
        {
            provider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
            provider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
            provider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();

            //AddTwoUsesWithClaims(provider);

            //this method will be executed only if we already have config file for IDS4
            //if not put it in comments
            var context = provider.GetRequiredService<ConfigurationDbContext>();
            PopulateDBWithConfig(context);

            //----------------------***************--------------------------
            //Uncomment after migrating IDS4DbContext
            var authSchemeProvider = provider.GetRequiredService<AuthSchemeProvider>();
            GetAllSchemes(authSchemeProvider).GetAwaiter().GetResult();
            //----------------------***************--------------------------
        }

        public void AddTwoUsesWithClaims(IServiceProvider provider)
        {
            {
                var userMgr = provider.GetRequiredService<UserManager<ApplicationUser>>();
                var alice = userMgr.FindByNameAsync("alice").Result;
                if (alice == null)
                {
                    alice = new ApplicationUser
                    {
                        UserName = "alice"
                    };
                    var result = userMgr.CreateAsync(alice, "Pass123$").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    alice = userMgr.FindByNameAsync("alice").Result;

                    result = userMgr.AddClaimsAsync(alice, new Claim[]{
                                new Claim(JwtClaimTypes.Name, "Alice Smith"),
                                new Claim(JwtClaimTypes.GivenName, "Alice"),
                                new Claim(JwtClaimTypes.FamilyName, "Smith"),
                                new Claim(JwtClaimTypes.Email, "AliceSmith@email.com"),
                                new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                                new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                                new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json)
                            }).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                    Console.WriteLine("alice created");
                }
                else
                {
                    Console.WriteLine("alice already exists");
                }

                var bob = userMgr.FindByNameAsync("bob").Result;
                if (bob == null)
                {
                    bob = new ApplicationUser
                    {
                        UserName = "bob"
                    };
                    var result = userMgr.CreateAsync(bob, "Pass123$").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    bob = userMgr.FindByNameAsync("bob").Result;
                    result = userMgr.AddClaimsAsync(bob, new Claim[]{
                                new Claim(JwtClaimTypes.Name, "Bob Smith"),
                                new Claim(JwtClaimTypes.GivenName, "Bob"),
                                new Claim(JwtClaimTypes.FamilyName, "Smith"),
                                new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
                            }).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                    Console.WriteLine("bob created");
                }
                else
                {
                    Console.WriteLine("bob already exists");
                }
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

        private async Task GetAllSchemes(AuthSchemeProvider authSchemeProvider)
        {
            await authSchemeProvider.LoadAllSchemes();
        }
    }
}
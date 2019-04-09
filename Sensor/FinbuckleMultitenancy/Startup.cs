using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rsk.AspNetCore.Authentication.Saml2p;

namespace FinbuckleMultitenancy
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(opt =>
            {
                opt.DefaultScheme = "Cookies";
                opt.DefaultChallengeScheme = "oidc";
            }).AddCookie("Cookies")
            .AddOpenIdConnect("oidc", o =>
            {
                o.SignInScheme = "Cookies";
                o.ClientId = "tenantApp";
                o.RequireHttpsMetadata = false;
                o.Authority = "http://localhost:33123";
                o.Scope.Add("openid");
                o.Scope.Add("profile");
                o.Scope.Add("role");
                o.Scope.Add("sensorsapi");
                o.Scope.Add("tenant");
                o.ResponseType = "id_token token";
                o.ClientSecret = "secret";
                o.ClaimActions.MapJsonKey("role","role","role");
                o.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = c =>
                    {
                        var tenant = c.Request.Host.Host;
                        c.ProtocolMessage.AcrValues = $"tenant:{tenant}";
                        return Task.FromResult(c);
                    }
                };
            })
              .AddSaml2p("saml2p", options => {
                  options.Licensee = "Demo";
                  options.LicenseKey = "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsImF1dGgiOiJERU1PIiwiZXhwIjoiMjAxOS0wNC0xMVQwMDowMDowMCIsImlhdCI6IjIwMTktMDMtMTFUMTE6NDk6NTguMDAwMDYwMiIsIm9yZyI6IkRFTU8iLCJhdWQiOjJ9.MoX1OmC40bJ/nlrdYaMdAW365sBQMOy4wGxe+50Sjrg2dFg9ZSM6GwGN0rB/cm43IiEDLaX/0XWdOA0/jQ/B3NofC7914GOSepKrYFFXE0NukdAQoq50lk5iUhfQe9jJD12+djGTgHHMBOPncWHvbLfxz2QJ8z59vGPL/Qh/gUTTrg3L5KYvg9tiVvOPDl092vKGwg8aTcQa/n61Csxt2dhCsJ6xYFK9HTYI1l2Lj0l1lFC6m4RtQ4Ip9CWQx8GBMMzz2tNzeh6QHw26DdFEABu4RFt3p90QKzp/h7R/ISCkC9AyVrAW688PYZ5hrhs8eD72Mm4OPn0Og0XdnDdJPSOJBVuTTY7+OCjNNKq69/PQjyhdjpIH1K8rjvfrq17G/k8SE5QDaGyZqui/wFbyFz6L4aLuxz0f7sydf69V2pZox6/Z8PkJezHGwxVbn8+UOs0e+7OY1nqWW41mgq1B+pDZX5xO+wAOkq4jXfqnlESZA0D6uIXYU2jRf2dB5VpgyZB0NrmyM2egpUTDxpEjkfOB9kYc53CWs2lGI48DlVzMaZ2jIWcqrqNcLSm8tPav152ftf1SJZewNIx+bdnb+kmk1IEe3+9APRYCfpiI3yk7eeaJcJmB6LXsGs1ATz7GhauAAYRW7I2LpzepzQ/JapG+KFiwfNAX2Q+Fk8rHpeI=";

                  options.IdentityProviderOptions = new IdpOptions
                  {
                      EntityId = "http://localhost:33123",
                      SigningCertificate = new X509Certificate2("idsrv3test.cer"),
                      SingleSignOnEndpoint = new SamlEndpoint("http://localhost:33123/saml/sso", SamlBindingTypes.HttpRedirect),
                      SingleLogoutEndpoint = new SamlEndpoint("http://localhost:33123/saml/slo", SamlBindingTypes.HttpRedirect),
                  };
                  options.ServiceProviderOptions = new SpOptions
                  {
                      EntityId = "http://test1.localhost:56995/saml",
                      MetadataPath = "/saml/metadata",
                      SignAuthenticationRequests = true,
                      SigningCertificate = new X509Certificate2("testclient.pfx", "test")
                  };

                  options.NameIdClaimType = "sub";
                  options.CallbackPath = "/signin-saml";
                  options.SignInScheme = "Cookies";
                  
              });

            services.AddMultiTenant()
                .WithHostStrategy().WithInMemoryStore(Configuration.GetSection("InMemoryStoreConfig"))
                .WithRemoteAuthentication()
                .WithPerTenantOptions<CookieAuthenticationOptions>((o, tenantInfo) =>
                {
                    o.Cookie.Name += tenantInfo.Id;
                    
                }).WithPerTenantOptions<AuthenticationOptions>((o, tenantInfo) =>
                {
                    o.DefaultChallengeScheme = tenantInfo.Items["Scheme"].ToString();
                    Console.WriteLine(o.DefaultChallengeScheme);
                })
                .WithPerTenantOptions<Saml2pAuthenticationOptions>((o, tenantInfo) =>
                {

                    o.ServiceProviderOptions = new SpOptions
                    {
                        EntityId = $"http://{tenantInfo.Identifier}.localhost:56995/saml",
                        MetadataPath = "/saml/metadata",
                        SignAuthenticationRequests = true,
                        SigningCertificate = new X509Certificate2("testclient.pfx", "test")
                    };

                });
            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("User", p => p.RequireClaim("role"));
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseMultiTenant();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

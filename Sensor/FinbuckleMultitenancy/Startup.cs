using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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
using Microsoft.IdentityModel.Tokens;
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

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
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
                o.Authority = "https://localhost:44374";
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
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "sub"
                };
            });

            services.AddMultiTenant()
                .WithHostStrategy().WithInMemoryStore(Configuration.GetSection("InMemoryStoreConfig"))
                .WithRemoteAuthentication()
                .WithPerTenantOptions<CookieAuthenticationOptions>((o, tenantInfo) =>
                {
                    o.Cookie.Name += tenantInfo.Identifier;
                }).WithPerTenantOptions<OpenIdConnectOptions>((o, tenantInfo) =>
                {
                    o.CallbackPath = $"/signin-oidc-{tenantInfo.Id}";
                });
            services.AddAuthorization();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
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

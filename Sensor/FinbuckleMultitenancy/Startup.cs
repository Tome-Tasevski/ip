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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
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
                o.ClaimActions.MapJsonKey("role", "role");
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
                    o.Cookie.Name += tenantInfo.Id;

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

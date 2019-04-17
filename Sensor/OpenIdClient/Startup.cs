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
using ServiceProviderMultiTenant.Services;
using Rsk.AspNetCore.Authentication.Saml2p;

namespace ServiceProviderMultiTenant
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

            services.AddScoped<ISensorDataHttpClient, SensorDataHttpClient>();
            services.AddHttpContextAccessor();
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);


            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            }).AddCookie("Cookies")
            .AddOpenIdConnect("oidc", opt =>
            {
                opt.SignInScheme = "Cookies";
                opt.Authority = "https://localhost:44374/";
                opt.RequireHttpsMetadata = false;
                opt.SaveTokens = true;
                opt.ClientId = "SpTenant";
                opt.Scope.Add("openid");
                opt.Scope.Add("profile");
                opt.Scope.Add("role");
                opt.Scope.Add("sensorsapi");
                opt.Scope.Add("tenant");
                opt.ResponseType = "id_token token";
                opt.ClientSecret = "secret";
                opt.ClaimActions.MapJsonKey("role", "role","role");
                opt.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = c =>
                    {
                        var tenant = c.Request.Host.Host;
                        c.ProtocolMessage.AcrValues = $"tenant:{tenant}";
                        return Task.FromResult(c);
                    }
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
                   o.SignedOutCallbackPath = $"/signout-callback-oidc-{tenantInfo.Id}";
                   o.SignedOutRedirectUri = $"https://{tenantInfo.Name}.localhost:44334";
               });
            services.AddAuthorization();
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

            app.UseMultiTenant();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
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

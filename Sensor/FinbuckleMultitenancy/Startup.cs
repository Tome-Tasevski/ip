using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
                options.CheckConsentNeeded = context => true;
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
            });

            services.AddMultiTenant()
                .WithHostStrategy().WithInMemoryStore(Configuration.GetSection("InMemoryStoreConfig"))
                .WithRemoteAuthentication()
                .WithPerTenantOptions<OpenIdConnectOptions>((o, tenantInfo) =>
                {
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

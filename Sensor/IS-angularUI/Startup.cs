using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;

namespace IS_angularUI
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services
                .AddIdentityServer()
                .AddTestUsers(Config.GetUsers())
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients())
                .AddSigningCredential(new X509Certificate2("idsrv3test.pfx", "idsrv3test"))
                .AddSamlPlugin(options => {
                    options.Licensee = "Demo";
                    options.LicenseKey = "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsImF1dGgiOiJERU1PIiwiZXhwIjoiMjAxOS0wNC0xMVQwMDowMDowMCIsImlhdCI6IjIwMTktMDMtMTFUMTE6NDk6NTguMDAwMDYwMiIsIm9yZyI6IkRFTU8iLCJhdWQiOjJ9.MoX1OmC40bJ/nlrdYaMdAW365sBQMOy4wGxe+50Sjrg2dFg9ZSM6GwGN0rB/cm43IiEDLaX/0XWdOA0/jQ/B3NofC7914GOSepKrYFFXE0NukdAQoq50lk5iUhfQe9jJD12+djGTgHHMBOPncWHvbLfxz2QJ8z59vGPL/Qh/gUTTrg3L5KYvg9tiVvOPDl092vKGwg8aTcQa/n61Csxt2dhCsJ6xYFK9HTYI1l2Lj0l1lFC6m4RtQ4Ip9CWQx8GBMMzz2tNzeh6QHw26DdFEABu4RFt3p90QKzp/h7R/ISCkC9AyVrAW688PYZ5hrhs8eD72Mm4OPn0Og0XdnDdJPSOJBVuTTY7+OCjNNKq69/PQjyhdjpIH1K8rjvfrq17G/k8SE5QDaGyZqui/wFbyFz6L4aLuxz0f7sydf69V2pZox6/Z8PkJezHGwxVbn8+UOs0e+7OY1nqWW41mgq1B+pDZX5xO+wAOkq4jXfqnlESZA0D6uIXYU2jRf2dB5VpgyZB0NrmyM2egpUTDxpEjkfOB9kYc53CWs2lGI48DlVzMaZ2jIWcqrqNcLSm8tPav152ftf1SJZewNIx+bdnb+kmk1IEe3+9APRYCfpiI3yk7eeaJcJmB6LXsGs1ATz7GhauAAYRW7I2LpzepzQ/JapG+KFiwfNAX2Q+Fk8rHpeI=";
                })

                .AddInMemoryServiceProviders(Config.GetServiceProviders());

            services.AddAuthentication()
                .AddOpenIdConnect("AAD", "Azure Active Directory", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                    options.Authority = "https://login.microsoftonline.com/common";
                    options.ClientId = "f6a6b204-ff96-4013-8adb-7b1ef0bdda2a";
                    //options.ClientId = "sensorclient"; //primer koga api-to ne e registrirano na azure
                    //options.ClientId = "oid client";
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("sensorsapi");
                    options.Scope.Add("role");
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false
                    };
                    options.GetClaimsFromUserInfoEndpoint = true;
                });

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddFile("Logs/{Date}-idsrv-log.txt");
            loggerFactory.AddDebug();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseIdentityServer()
                .UseIdentityServerSamlPlugin();

            app.UseAuthentication();

            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.Quickstart.UI;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdSrv.Data;
using IdSrv.Data.Models;
using IdSrv.Quickstart.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdSrv.Quickstart.Configuration
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : Controller
    {
        private readonly AuthSchemeProvider _authSchemeProvider;
        private readonly IClientStore _clientStore;
        private readonly Repository _repo;
        private readonly IConfigurationDbContext _configurationDbContext;

        public ConfigurationController(AuthSchemeProvider authSchemeProvider, Repository repo, IConfigurationDbContext configurationDbContext, IClientStore clientStore)
        {
            _authSchemeProvider = authSchemeProvider;
            _repo = repo;
            _configurationDbContext = configurationDbContext;
            _clientStore = clientStore;
        }

        [HttpPost("addscheme")]
        public async Task<IActionResult> AddScheme(string tenantId)
        {
            await _authSchemeProvider.AddOrUpdate(tenantId);
            return Redirect("/");
        }

        [HttpPost("addconfing")]
        public IActionResult AddTenantConfig(AADConfig cfg, string tenantId)
        {
            var tenant = _repo.GetTenantById(tenantId);
            if(tenant.Protocol.Equals("oidc"))
            {
                var oidConfig = new OpenIDConfig
                {
                    OpenId = "2",
                    SignInScheme = cfg.SignInScheme,
                    SignOutScheme = cfg.SignOutScheme,
                    Authority = $"{cfg.Authority}/{cfg.DirectoryId}",
                    ClientId = cfg.ClientId,
                    Tenant = tenant,
                };
                _repo.AddOIDConfig(oidConfig);
                foreach(var client in _configurationDbContext.Clients.AsQueryable().Include("RedirectUris"))
                {
                    client.RedirectUris.Add(new ClientRedirectUri() { Client = client, RedirectUri = $"https://{tenant.Name}.localhost:{(client.Id == 1 ? "44372" : "44334")}/signin-oidc" });
                    _configurationDbContext.Clients.Attach(client).State = EntityState.Modified;
                }
                _configurationDbContext.SaveChanges();
            }
            return Ok();
        }
    }
}
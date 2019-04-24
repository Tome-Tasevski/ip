using System;
using System.Threading.Tasks;
using IdentityServer4.Stores;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Data.Models;
using IdentityServerAspNetIdentity.Quickstart.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServerAspNetIdentity.Quickstart.Configuration
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : Controller
    {
        private readonly AuthSchemeProvider _authSchemeProvider;
        private readonly IClientStore _clientStore;
        private readonly Repository _repo;

        public ConfigurationController(AuthSchemeProvider authSchemeProvider, Repository repo, IClientStore clientStore)
        {
            _authSchemeProvider = authSchemeProvider;
            _repo = repo;
            _clientStore = clientStore;
        }

        [HttpPost("addscheme")]
        public async Task<IActionResult> AddScheme(string tenantId)
        {
            await _authSchemeProvider.AddOrUpdate(tenantId);
            return Redirect("/");
        }

        [HttpPost("newOidConfig")]
        public IActionResult AddOpenIDConfig(OIDConfig cfg, string tenantId)
        {
            var tenant = _repo.GetTenantById(tenantId);
            if (tenant.Protocol.Equals("oidc"))
            {
                var oidConfig = new OpenIDConfig
                {
                    Authority = cfg.DirectoryId == null ? cfg.Authority : $"{cfg.Authority}/{cfg.DirectoryId}/",
                    ClientId = cfg.ClientId,
                    ClientSecret = cfg.ClientSecret ?? "",
                    Tenant = tenant,
                };

                _repo.AddOIDConfig(oidConfig);
            }
            return Ok();
        }

        [HttpPost("newSamlConfig")]
        public IActionResult AddSamlConfig(SMLConfig cfg, string tenantId)
        {
            var tenant = _repo.GetTenantById(tenantId);
            if (tenant.Protocol.Equals("saml"))
            {
                var samlcfg = new SamlConfig
                {
                    IdpEntityId = cfg.IdpEntityId,
                    IdpSigningCertificate = cfg.IdpSigningCertificate,
                    SingleLogoutEndpoint = cfg.SLOEndpoint,
                    SingleSignOnEndpoint = cfg.SSOEndpoint,
                    Tenant = tenant
                };

                _repo.AddSamlConfig(samlcfg);
            }
            return Ok();
        }

    }
}
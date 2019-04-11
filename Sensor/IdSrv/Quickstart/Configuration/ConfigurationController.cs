using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Quickstart.UI;
using IdentityServer4.Services;
using IdSrv.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Rsk.AspNetCore.Authentication.Saml2p;

namespace IdSrv.Quickstart.Configuration
{
    [SecurityHeaders]
    public class ConfigurationController : Controller
    {
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IOptionsMonitorCache<OpenIdConnectOptions> _openIdOptions;
        //private readonly OpenIdConnectPostConfigureOptions _oidPostConfOptions;
        private readonly IOptionsMonitorCache<Saml2pAuthenticationOptions> _saml2pOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Repository _repo;
        private readonly IIdentityServerInteractionService _interaction;

        public ConfigurationController(IAuthenticationSchemeProvider schemeProvider, 
            IOptionsMonitorCache<OpenIdConnectOptions> openIdOptions,
            IOptionsMonitorCache<Saml2pAuthenticationOptions> saml2pOptions, 
            IHttpContextAccessor httpContextAccessor,
            IIdentityServerInteractionService interaction,
            //OpenIdConnectPostConfigureOptions oidPostConfOptions,
            Repository repo)
        {
            _schemeProvider = schemeProvider;
            _openIdOptions = openIdOptions;
            _saml2pOptions = saml2pOptions;
            _httpContextAccessor = httpContextAccessor;
            _interaction = interaction;
            _repo = repo;
            //_oidPostConfOptions = oidPostConfOptions;
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdate(string scheme, string optionsMessage)
        {
            var features = _httpContextAccessor.HttpContext.Features.Get<IHttpRequestFeature>();
            var returnUrl = features.RawTarget;
            //var authContext = _interaction.GetAuthorizationContextAsync(returnUrl).Result;

            //var currentRequestTenant = authContext.Tenant.Split(".").First();

            var tenant = "3";
            var config = _repo.GetOpenIdConfig(tenant);
            var oidOptions = new OpenIdConnectOptions
            {
                SignInScheme = config.SignInScheme,
                SignOutScheme = config.SignOutScheme,
                RequireHttpsMetadata = false,
                Authority = config.Authority,
                ClientId = config.ClientId,
                GetClaimsFromUserInfoEndpoint = true
            };

            if (await _schemeProvider.GetSchemeAsync(scheme) == null)
            {
                _schemeProvider.AddScheme(new AuthenticationScheme(scheme, scheme, typeof(OpenIdConnectHandler)));
            }
            else
            {
                _openIdOptions.TryRemove(scheme);
            }
            _openIdOptions.TryAdd(scheme, oidOptions);
            return Redirect("/");
        }
    }
}
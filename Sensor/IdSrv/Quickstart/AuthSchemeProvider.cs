using IdentityModel;
using IdentityModel.Client;
using IdentityServer4.Saml.Configuration;
using IdentityServer4.Services;
using IdSrv.Data;
using IdSrv.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Rsk.AspNetCore.Authentication.Saml2p;
using Rsk.AspNetCore.Authentication.Saml2p.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IdSrv.Quickstart
{
    public class AuthSchemeProvider
    {
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IOptionsMonitorCache<OpenIdConnectOptions> _openIdOptions;
        private readonly IOptionsMonitorCache<Saml2pAuthenticationOptions> _saml2pOptions;
        private readonly IOptionsMonitorCache<SpOptions> _samlSpOptions;
        private readonly IOptionsMonitorCache<IdpOptions> _samlIdpOptions;
        private readonly OpenIdConnectPostConfigureOptions _oidPostConfOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Repository _repo;
        private readonly IIdentityServerInteractionService _interaction;

        public AuthSchemeProvider(IAuthenticationSchemeProvider schemeProvider,
            IOptionsMonitorCache<OpenIdConnectOptions> openIdOptions,
            OpenIdConnectPostConfigureOptions oidPostConfOptions,
            IOptionsMonitorCache<Saml2pAuthenticationOptions> saml2pOptions,
            IHttpContextAccessor httpContextAccessor,
            IIdentityServerInteractionService interaction,
            IOptionsMonitorCache<SpOptions> samlSpOptions,
            IOptionsMonitorCache<IdpOptions> samlIdpOptions,
            Repository repo)
        {
            _schemeProvider = schemeProvider;
            _openIdOptions = openIdOptions;
            _saml2pOptions = saml2pOptions;
            _httpContextAccessor = httpContextAccessor;
            _interaction = interaction;
            _repo = repo;
            _oidPostConfOptions = oidPostConfOptions;
            _samlSpOptions = samlSpOptions;
            _samlIdpOptions = samlIdpOptions;
        }

        public async Task AddOrUpdate(string tenantId)
        {
            var scheme = $"{tenantId}-scheme";
            var oidOptions = new OpenIdConnectOptions();
            var samlOptions = new Saml2pAuthenticationOptions();
            var saml2SpOptions = new SpOptions();
            var saml2IdpOptions = new IdpOptions();
            var tenant = _repo.GetAllTenants().FirstOrDefault(x => x.TenantId.Equals(tenantId));
            var oidcProtocol = tenant.Protocol.Equals("oidc");
            if (tenant != null && oidcProtocol)
            {
                var oidConfig = _repo.GetOpenIdConfig(tenantId);
                oidOptions = BuildOidOptions(oidConfig);

            }else
            {
                var samlConfig = _repo.GetSamlConfig(tenantId);
                saml2SpOptions = new SpOptions()
                {
                    EntityId = "https://localhost:44374/saml",
                    SigningCertificate = new X509Certificate2("testclient.pfx", "test"),
                    MetadataPath = "/saml/metadata",
                    SignAuthenticationRequests = true
                };
                saml2IdpOptions = new IdpOptions()
                {
                    EntityId = samlConfig.IdpEntityId,
                    SingleSignOnEndpoint = new SamlEndpoint(samlConfig.SingleSignOnEndpoint, SamlBindingTypes.HttpPost),
                    SingleLogoutEndpoint = new SamlEndpoint(samlConfig.SingleLogoutEndpoint, SamlBindingTypes.HttpPost),
                    SigningCertificate = new X509Certificate2(samlConfig.IdpSigningCertificate), //file name
                };

                samlOptions = BuildSamlOptions(samlConfig, saml2SpOptions, saml2IdpOptions);
            }
            
            if (await _schemeProvider.GetSchemeAsync(scheme) == null)

            {
                _schemeProvider.AddScheme(new AuthenticationScheme(scheme, scheme, oidcProtocol ? typeof(OpenIdConnectHandler) : typeof(Saml2pAuthenticationHandler)));
            }
            else
            {
                if (oidcProtocol)
                    _openIdOptions.TryRemove(scheme);
                else
                    _saml2pOptions.TryRemove(scheme);
            }
            if(oidcProtocol)
            {
                _oidPostConfOptions.PostConfigure(scheme, oidOptions);
                _openIdOptions.TryAdd(scheme, oidOptions);
            }else
            {
                _saml2pOptions.TryAdd(scheme, samlOptions);
            }
        }

        public async Task LoadAllSchemes()
        {
            var tenants = _repo.GetAllTenants().Where(t => t.LoginType.Equals("external"));
            if(tenants != null)
            {
                foreach (var tenant in tenants.ToList())
                {
                    await AddOrUpdate(tenant.TenantId);
                }
            }
        }

        private OpenIdConnectOptions BuildOidOptions(OpenIDConfig config)
        {
            return new OpenIdConnectOptions
            {
                SignInScheme = "idsrv.external",
                SignOutScheme = "idsrv",
                RequireHttpsMetadata = false,
                Authority = config.Authority,
                ClientId = config.ClientId,
                ClientSecret = config.ClientSecret ?? "",
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    NameClaimType = JwtClaimTypes.Subject,
                    RoleClaimType = JwtClaimTypes.Role,
                },
                SaveTokens = true,
                CallbackPath = $"/signin-oidc-{config.TenantId}"
            };
        }

        private Saml2pAuthenticationOptions BuildSamlOptions(SamlConfig config, SpOptions spOptions, IdpOptions idpOptions)
        {
            return new Saml2pAuthenticationOptions
            {
                Licensee = "Demo",
                LicenseKey = "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsImF1dGgiOiJERU1PIiwiZXhwIjoiMjAxOS0wOS0xMlQwMTowMDowMS4wODkwNTkzKzAxOjAwIiwiaWF0IjoiMjAxOS0wOC0xM1QwMDowMDowMS4wMDAwNTkzIiwib3JnIjoiREVNTyIsImF1ZCI6Mn0=.aIIw1/atlaiBci5zSxPIhdinI7tEmBZYK3Xuhe19a5twDeSzRm/i57gWpMMu/2x8O40nfAi9yk2Gbpadzv8RpOxaF5+OnnIcg4yevKLipJKZJ9bBDDC78Yl/nkhM6N65e18haUmgO/l8EYeRpYNTBE7dNGK2wZJAyqDpWpBYQ8tbCvfjchj5GGqudqFT/YLNuHn1y0Ps7mP4rh4V5G7TyIaY9QfW3ZMGJGtOQyjvTzlc02IZrw6idhRpNNazMqZAFnUKUV+bR//jyx1uBkz1H+hF0eslQt9Wt9yse3JAZi/D4oUTFyPOfzzhGkW2ceftX8tmYEWE/lPjQ3+xonKX+XNxJEXjib6d6GkTJpALpBniXS6XIKDnDV2h0Bs14lMA+2ddEDBMWVid4ubdUBZBkQLKWfZkPACUd6aBfx001v3QE2YQ9yxzdq0EAWnvEV5EE3NR4sLaNNuYxxMSoBs8ZRovhcT7Yo+o6skNu1cVvGVjOl+YMURdQgc7dPU+NxNG4oekEyW+i9N3RidvZeM1xz8dQipBMCMrrOymLV5hT32N9Y/XX4yC26zbuc5aYoaQKNqF7ZVfbrOa2H4G/Jr3pYj3EWRWfy2xlLDbUSzjcJ4ut2OZFHs5ql2Ut9y/LQ8yB3Impur6h8TpwbziWza/z2j2Gmj3rwKbN0vO2zYQJv4=",
                IdentityProviderOptions = idpOptions,
                ServiceProviderOptions = spOptions,

                //SaveTokens = true,
                NameIdClaimType = "sub",
                CallbackPath = $"/signin-saml-{config.TenantId}",
                SignInScheme = "idsrv.external",
                ClaimsIssuer = idpOptions.EntityId,
            };
        }

    }
}

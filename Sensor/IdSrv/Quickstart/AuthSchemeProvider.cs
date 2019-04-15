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
                    EntityId = samlConfig.SpEntityId,
                    SigningCertificate = new X509Certificate2(samlConfig.SpSigningCertificate.Split(" ")[0], samlConfig.SpSigningCertificate.Split(" ")[1]),
                    MetadataPath = samlConfig.MetadataPath,
                    SignAuthenticationRequests = true
                };
                saml2IdpOptions = new IdpOptions()
                {
                    EntityId = samlConfig.IdpEntityId,
                    SingleSignOnEndpoint = new SamlEndpoint(samlConfig.SingleSignOnEndpoint, SamlBindingTypes.HttpRedirect),
                    SingleLogoutEndpoint = new SamlEndpoint(samlConfig.SingleLogoutEndpoint, SamlBindingTypes.HttpRedirect),
                    SigningCertificate = new X509Certificate2(samlConfig.IdpSigningCertificate),
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
            foreach (var tenant in tenants.ToList())
            {
                await AddOrUpdate(tenant.TenantId);
            }
        }

        private OpenIdConnectOptions BuildOidOptions(OpenIDConfig config)
        {
            return new OpenIdConnectOptions
            {
                SignInScheme = config.SignInScheme,
                SignOutScheme = config.SignOutScheme,
                RequireHttpsMetadata = false,
                Authority = config.Authority,
                ClientId = config.ClientId,
                GetClaimsFromUserInfoEndpoint = true,
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
                Licensee = config.Licensee,
                LicenseKey = config.LicenseKey,
                IdentityProviderOptions = idpOptions,
                ServiceProviderOptions = spOptions,

                SaveTokens = true,
                NameIdClaimType = "sub",
                CallbackPath = "/signin-saml",
                SignInScheme = config.SignInScheme,
                ClaimsIssuer = idpOptions.EntityId,

                //TimeComparisonTolerance = config.TimeComparisonTolerance
            };
        }

    }
}

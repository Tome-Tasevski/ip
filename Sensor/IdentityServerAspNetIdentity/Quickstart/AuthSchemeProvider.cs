using IdentityModel;
using IdentityModel.Client;
using IdentityServer4.Saml.Configuration;
using IdentityServer4.Services;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Data.Models;
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

namespace IdentityServerAspNetIdentity.Quickstart
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

            }
            else
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
                    SingleSignOnEndpoint = new SamlEndpoint(samlConfig.SingleSignOnEndpoint, SamlBindingTypes.HttpRedirect),
                    SingleLogoutEndpoint = new SamlEndpoint(samlConfig.SingleLogoutEndpoint, SamlBindingTypes.HttpRedirect),
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
            if (oidcProtocol)
            {
                _oidPostConfOptions.PostConfigure(scheme, oidOptions);
                _openIdOptions.TryAdd(scheme, oidOptions);
            }
            else
            {
                _saml2pOptions.TryAdd(scheme, samlOptions);
            }
        }

        public async Task LoadAllSchemes()
        {
            var tenants = _repo.GetAllTenants().Where(t => t.LoginType.Equals("external"));
            if (tenants != null)
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
                LicenseKey = "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsImF1dGgiOiJERU1PIiwiZXhwIjoiMjAxOS0wNS0xMVQwMTowMDowMC45Njc1MTI5KzAxOjAwIiwiaWF0IjoiMjAxOS0wNC0xMVQwMDowMDowMC4wMDA1MTI5Iiwib3JnIjoiREVNTyIsImF1ZCI6Mn0=.PKaOIARp3KB5GYWCoBXkJHOSqpGg2BcPpQX9/N/4SJ9rFyI3CLGILF/qrQzWBBUdTTKxMGU4LaPToCQE3XAF1M4Ikh8QCYNMsS2o5OlfO2+sqmVvI6Y8ucjMgwPnEigFW+q1+mZbWHlqPto0OsHhSjX8PgZb+g8nsWbGb5MLSAaM+8bLgcghizj3xZctr6QyOI0a9p9VThzPSNi4hKEyPBZ/EOjt7Qxh2R4TVsY9TnbeTIRT+P9yGXGQoJqmIIqVhlMu7v6Qe0hV1orgLKonJpqQVRsYUK/rl9ygyMV7lfB3KQ4k3EwqsUFF9OclMF+DZBNa1YOfqBmYnVebQWmpvkqxh/RiEiRrh+ERoGDNrOBgPbPlNj80dxy37rkqZSFyNg6su3F7v3ZyjFyS9kVha0HsnhkvN8Kz14myzokxwiBe2BVDy7ErzXajhP3q8a04SP6qL22mO9uBwAppHPD7UOU88+CC3GHVD5NmSjzMwN2sNzgExjOQ4dFDjvfcz9byilMUPCW1o3vRcIVA7CJx7F28ZYQFw/LuBlTPcZ9LlkWq1LptZR1KeusKX5vcz5QvWj7F+uO1fA2uwrUSdghGGkHbfLrh6OJmUFUD8HGm0N83ydLQKaEMMJgeA4T3ox19zycws7RdrZ6uLX2hTySCZ5xjf6eUu97QDxxd/LEnQvU=",
                IdentityProviderOptions = idpOptions,
                ServiceProviderOptions = spOptions,

                SaveTokens = true,
                NameIdClaimType = "sub",
                CallbackPath = $"/signin-saml-{config.TenantId}",
                SignInScheme = "idsrv.external",
                ClaimsIssuer = idpOptions.EntityId,
            };
        }
    }
}

using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Quickstart.UI;
using IdentityServer4.Saml;
using IdentityServer4.Saml.Models;
using IdentityServer4.Test;

namespace IdSrv
{
    public static class Config
    {
        public static List<TestUser> GetUsers()
        {
            return TestUsers.Users;
        }


        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource("role","Role", new List<string> { ClaimTypes.Role }),
                new IdentityResource("tenant", new List<string> { "tenant" })
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource> {
                new ApiResource("sensorsapi",  new List<string> {"role"})

            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "tenantApp",
                    ClientName = "tenant Client",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    RedirectUris = { },
                    PostLogoutRedirectUris = {},
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "sensorsapi",
                        "role",
                        "tenant"
                    },
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                   // ProtocolType = IdentityServerConstants.ProtocolTypes.Saml2p,
                },
                new Client
                {
                    ClientId = "SpTenant",
                    ClientName = "SP Client",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    RedirectUris =
                    {
                    },
                    PostLogoutRedirectUris = {  },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "sensorsapi",
                        "role",
                        "tenant"
                    },
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                }
            };
        }

        public static List<ServiceProvider> GetServiceProviders()
        {
            return new List<ServiceProvider>
            {
                
            };
        }
    }
}
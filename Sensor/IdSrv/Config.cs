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
                new IdentityResource("role", "Role", new List<string> {JwtClaimTypes.Role, ClaimTypes.Role }),
                new IdentityResource("tenant", new List<string> { "tenant" })
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource> {
                new ApiResource("sensorsapi", JwtClaimTypes.Role)

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
                    RedirectUris = { "https://test1.localhost:44372/signin-oidc", "https://test2.localhost:44372/signin-oidc","https://test3.localhost:44372/signin-oidc" },
                    PostLogoutRedirectUris = {
                        "https://test2.localhost:44372/signout-callback-oidc",
                        "https://test3.localhost:44372/signout-callback-oidc",
                        "https://test1.localhost:44372/signout-callback-oidc"},
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
                        "https://test1.localhost:44334/signin-oidc",
                        "https://test2.localhost:44334/signin-oidc",
                        "https://test3.localhost:44334/signin-oidc"
                    },
                    PostLogoutRedirectUris = { "http://test1.localhost:44334/signout-callback-oidc", "http://test2.localhost:44334/signout-callback-oidc", "http://test3.localhost:44334/signout-callback-oidc" },
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
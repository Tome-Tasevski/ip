// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Saml.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace IDS4Admin
{
    public static class Config
    {
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
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Saml.Models;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using IdentityServer4.Saml;



namespace IdServerHost
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
        {
            new ApiResource("api1", "My API")
        };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
        {

            new Client
            {
                ClientId = "client",
                AllowedGrantTypes = GrantTypes.ClientCredentials,

                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                AllowedScopes = { "api1" }
            },

             new Client
        {
            ClientId = "mvc",
            AllowedGrantTypes = GrantTypes.Hybrid,
            ClientSecrets = { new Secret("secret".Sha256()) },
            RedirectUris = { "http://localhost:44354/signin-oidc" },
            PostLogoutRedirectUris = { "http://localhost:44354/signout-callback-oidc" },

            AllowedScopes = new List<string>
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                "api1"
            },
            AllowOfflineAccess = true
        },

             new Client {
              ClientId = "http://localhost:52430/saml",
              ClientName = "RSK SAML2P Test Client",
              ProtocolType = IdentityServerConstants.ProtocolTypes.Saml2p,
              AllowedScopes = { "openid", "profile" , "sensorsapi" }
            },

 
        };
        }

        public static IEnumerable<ServiceProvider> GetServiceProviders()
        {
            return new[]
            {
                new ServiceProvider
                {
                    EntityId = "http://localhost:52430/saml",
                    AssertionConsumerServices =
                        {new Service(SamlConstants.BindingTypes.HttpPost, "http://localhost:52430/signin-saml")},
                    SigningCertificates = {new X509Certificate2("testclient.cer")}
                }
            };
        }
    }
}
   
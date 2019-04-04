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


        /*se definiraat resursite shto gi chuva Identity serverot*/
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),//sopstvenoto unique userId, i kje bide istata vrednost koga i da se najavi uesrot na aplikacijata
                new IdentityResources.Profile(),// nekolku properia za userot: firstName, lastName, displayName, url....
                new IdentityResource("role", "Role", new List<string> {JwtClaimTypes.Role, ClaimTypes.Role })//a mozat da se definiraat i svoi IdentityResources
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
                    ClientName = "Sensor Dashboard",
                    ClientId = "sensorclient",
                    /*protokolite (vo ovoj sluchaj openId) imaat flow. Toa flow go opishuva tipot na aplikacijata i nachinot na
                     komunikacija pomekju aplikacijata i token serverot (IS4)
                     Implicit znachi deka komunikacijata kje se odviva samo preku browser (t.e samo preku avtorizirani edn pointi)*/
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    RedirectUris =
                    {
                        /*url na app kade shto se dobivaat rezultatite i toa ne bilo kade tuku na komponenta od app shto kje komunicira so ovoj protokol
                         u ovoj sluchaj toa e openId connect middleware od microsoft, signin-oidc*/
                        "http://localhost:33117/signin-oidc"
                    },
                    PostLogoutRedirectUris =
                    {
                        "http://localhost:33117/signout-callback-oidc"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "sensorsapi",
                        "role"
                    },
                    IdentityProviderRestrictions = new List<string>() { }
                    ,RequireConsent = false // ako ne sakame da se prikazuva skreenot so "vie imate permisii na ..", so ova potvrduvame deka sme ok token serverot da gi dostavi 
                                                //ovie podatoci na aplikacijata (vo nashiot slucha Client)
                    ,EnableLocalLogin = true,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true
                },
                new Client {
                      ClientId = "https://test1.localhost:44379/saml",
                      ClientName = "RSK SAML2P Test Client",
                      ProtocolType = IdentityServerConstants.ProtocolTypes.Saml2p,
                      AllowedScopes = { "openid", "profile", "role"}
                },
                new Client {
                      ClientId = "https://test2.localhost:44379/saml",
                      ClientName = "RSK SAML2P Test Client",
                      ProtocolType = IdentityServerConstants.ProtocolTypes.Saml2p,
                      AllowedScopes = { "openid", "profile", "role"}
                },
                new Client
                {
                    ClientId = "tenantApp",
                    ClientName = "tenant Client",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    RedirectUris = { "https://test1.localhost:44367/signin-oidc", "https://test2.localhost:44367/signin-oidc" },
                    PostLogoutRedirectUris = { "https://test1.localhost:44367/signout-callback-oidc", "https://test2.localhost:44367/signout-callback-oidc" },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "sensorsapi",
                        "role"
                    },
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true
                }
            };
        }

        public static List<ServiceProvider> GetServiceProviders()
        {
            return new List<ServiceProvider>
            {
                new ServiceProvider {
                      EntityId = "https://test1.localhost:44379/saml",
                      SigningCertificates = {new X509Certificate2("TestClient.cer")},
                      AssertionConsumerServices = { new Service(SamlConstants.BindingTypes.HttpPost, "https://test1.localhost:44379/signin-saml") },
                },
                new ServiceProvider {
                      EntityId = "https://test2.localhost:44379/saml",
                      SigningCertificates = {new X509Certificate2("TestClient.cer")},
                      AssertionConsumerServices = { new Service(SamlConstants.BindingTypes.HttpPost, "https://test1.localhost:44379/signin-saml") },
                }
        };
        }
    }
}
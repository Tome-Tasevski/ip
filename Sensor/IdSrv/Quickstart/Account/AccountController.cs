// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using IdSrv.Data;
using IdSrv.Data.Models;

namespace IdentityServer4.Quickstart.UI
{
    /// <summary>
    /// This sample controller implements a typical login/logout/provision workflow for local and external accounts.
    /// The login service encapsulates the interactions with the user data store. This data store is in-memory only and cannot be used for production!
    /// The interaction service provides a way for the UI to communicate with identityserver for validation and context retrieval
    /// </summary>
    [SecurityHeaders]
    public class AccountController : Controller
    {
        private readonly Repository _repo;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;
        private readonly AccountService _account;

        public AccountController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IHttpContextAccessor httpContextAccessor,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            Repository repo)
        {
            _repo = repo;
            _interaction = interaction;
            _events = events;
            _account = new AccountService(interaction, httpContextAccessor, schemeProvider, clientStore, repo);
        }

        /// <summary>
        /// Show login page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await _account.BuildLoginViewModelAsync(returnUrl);
            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return await ExternalLogin(vm.ExternalLoginScheme, returnUrl);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
            if (button != "login")
            {
                // the user clicked the "cancel" button
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // since we don't have a valid context, then we just go back to the home page
                    return Redirect("~/");
                }
            }


            if (ModelState.IsValid)
            {
                if (_repo.ValidateCredentials(model.Username, model.Password))
                {
                    var user = _repo.FindByUsername(model.Username);
                    var claims = new List<Claim>();
                    if (user != null)
                    {
                        await _events.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.UserId, user.Username));
                        foreach (var claim in _repo.GetUserClaims(user.UserId))
                        {
                            claims.Add(new Claim(claim.Claims.Type, claim.Value));
                        }
                    }
                    
                    // only set explicit expiration here if user chooses "remember me". 
                    // otherwise we rely upon expiration configured in cookie middleware.
                    AuthenticationProperties props = null;
                    if (AccountOptions.AllowRememberLogin && model.RememberLogin)
                    {
                        props = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                        };
                    };
                    // issue authentication cookie with subject ID and username
                    await HttpContext.SignInAsync(user.UserId, user.Username, props, claims.ToArray());

                    // make sure the returnUrl is still valid, and if so redirect back to authorize endpoint or a local page
                    if (_interaction.IsValidReturnUrl(model.ReturnUrl) || Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return Redirect("~/");
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials"));

                ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
            }

            // something went wrong, show form with error
            var vm = await _account.BuildLoginViewModelAsync(model);
            return View(vm);
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExternalLogin(string provider, string returnUrl)
        {
            var props = new AuthenticationProperties()
            {
                RedirectUri = Url.Action("ExternalLoginCallback"),//otkako kje se vrati response od token serverot i posle validacija na id tokenot, da se redirectira userot na odredena lokacija 
                Items =
                {
                    { "returnUrl", returnUrl }
                }
            };

            // windows authentication needs special handling
            // since they don't support the redirect uri, 
            // so this URL is re-triggered when we call challenge
            if (AccountOptions.WindowsAuthenticationSchemeName == provider)
            {
                // see if windows auth has already been requested and succeeded
                var result = await HttpContext.AuthenticateAsync(AccountOptions.WindowsAuthenticationSchemeName);
                if (result?.Principal is WindowsPrincipal wp)
                {
                    props.Items.Add("scheme", AccountOptions.WindowsAuthenticationSchemeName);

                    var id = new ClaimsIdentity(provider);
                    id.AddClaim(new System.Security.Claims.Claim(JwtClaimTypes.Subject, wp.Identity.Name));
                    id.AddClaim(new System.Security.Claims.Claim(JwtClaimTypes.Name, wp.Identity.Name));

                    // add the groups as claims -- be careful if the number of groups is too large
                    if (AccountOptions.IncludeWindowsGroups)
                    {
                        var wi = wp.Identity as WindowsIdentity;
                        var groups = wi.Groups.Translate(typeof(NTAccount));
                        var roles = groups.Select(x => new System.Security.Claims.Claim(JwtClaimTypes.Role, x.Value));
                        id.AddClaims(roles);
                    }

                    await HttpContext.SignInAsync(
                        IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme,
                        new ClaimsPrincipal(id),
                        props);
                    return Redirect(props.RedirectUri);
                }
                else
                {
                    // challenge/trigger windows auth
                    return Challenge(AccountOptions.WindowsAuthenticationSchemeName);
                }
            }
            else
            {
                // start challenge and roundtrip the return URL
                props.Items.Add("scheme", provider);
                return Challenge(props, provider);
            }
        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            // read external identity from the temporary cookie
            var result = await HttpContext.AuthenticateAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);
            var returnUrl = result.Properties.Items["returnUrl"];
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            // retrieve claims of the external user
            var externalUser = result.Principal;
            var claims = externalUser.Claims.ToList();

            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            var upn = claims.FirstOrDefault(x => x.Type.Equals("upn"));
            if (upn == null)
            {
                upn = claims.FirstOrDefault(x => x.Type.Equals("userPrincipalName"));
            }
            if (upn == null)
            {
                throw new Exception("Unknown userid");
            }

            // remove the user id claim from the claims collection and move to the userId property
            // also set the name of the external authentication provider
            claims.Remove(upn);
            var provider = result.Properties.Items["scheme"];
            var userId = upn.Value;

            // this is where custom logic would most likely be needed to match your users from the
            // external provider's authentication result, and provision the user as you see fit.
            // 
            // check if the external user is already provisioned
            var user = _repo.FindByExternalProvider(provider, userId);

	        if (user != null)
            {
                foreach (var claim in _repo.GetUserClaims(user.UserId))
                {
                    claims.Add(new Claim(claim.Claims.Type, claim.Value));
                }
            }

            List<UserClaims> userClaims = new List<UserClaims>();
            if (user == null)
            {
                List<Claims> dbClaims = _repo.GetClaims();
               
                claims.ForEach(claim =>
                {
                    var c = dbClaims.Where(x => x.Type.Equals(claim.Type)).FirstOrDefault();
                    if ( c != null)
                    {
                        var id = c.ClaimId;
                        userClaims.Add(new UserClaims
                        {
                            UserId = userId,
                            ClaimId = id,
                            Value = claim.Value
                        });
                    }
                   
                });
                userClaims.Add(new UserClaims
                {
                    UserId = userId,
                    ClaimId = "20",
                    Value = "User"
                });
                claims.Add(new Claim(JwtClaimTypes.Role, "User"));
                user = new IS4User()
                {
                    ExternalUserId = userId,
                    IsExternalUser = true,
                    Tenant = _repo.GetTenant(context.Tenant.Split(".").First()),
                    Provider = provider,
                    Claims = userClaims
                };
                _repo.RegisterUser(user);
            }

            var tenant = new Claim("tenant", context.Tenant.Split(".").First());
            userClaims.Add(new UserClaims
            {
                UserId = userId,
                ClaimId = "17",
                Value = context.Tenant.Split(".").First()
            });
            claims.Add(tenant);

            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            var sid = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                claims.Add(new System.Security.Claims.Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            AuthenticationProperties props = null;
            var id_token = result.Properties.GetTokenValue("id_token");
            if (id_token != null)
            {
                props = new AuthenticationProperties();
                props.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = id_token } });
            }
            // issue authentication cookie for user
            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, userId, user.UserId, user.Username));
            await HttpContext.SignInAsync(userId, user.Username, provider, props, claims.ToArray());

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);

            // validate return URL and redirect back to authorization endpoint or a local page
            if (_interaction.IsValidReturnUrl(returnUrl) || Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("~/");
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await _account.BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await _account.BuildLoggedOutViewModelAsync(model.LogoutId);

            var user = HttpContext.User;
            if (user?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await HttpContext.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(user.GetSubjectId(), user.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);
        }
    }
}
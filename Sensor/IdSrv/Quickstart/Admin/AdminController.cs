using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using IdSrv.Data;
using IdSrv.Data.Models;
using IdSrv.Quickstart.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdSrv.Quickstart.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly Repository _repo;
        private readonly IConfigurationDbContext _configurationDbContext;

        public AdminController(Repository repo, IConfigurationDbContext configurationDbContext)
        {
            _repo = repo;
            _configurationDbContext = configurationDbContext;
        }

        [HttpPost("new-client")]
        public async Task<IActionResult> AddClient(NewClient client, [FromQuery(Name = "serviceProviders")]List<int> serviceProviders)
        {
            var newTenant = new IS4Tenant
            {
                Name = client.Name,
                LoginType = client.LoginType,
                Protocol = client.Protocol,
            };

            try
            {
                await _repo.AddClient(newTenant);
                UpdateClientConfig(newTenant, serviceProviders);
                return Ok();
            }catch
            {
                return BadRequest();
            }
        }

        [HttpPost("new-User")]
        public async Task<IActionResult> AddUserAsync(User user, string tenantId)
        {
            var tenant = _repo.GetTenantById(tenantId);
            var newUser = new IS4User
            {
                Username = user.Username,
                Password = user.Password,
                IsExternalUser = user.IsExternalUser,
                Provider = user.Provider,
                ExternalUserId = user.ExternalUserId,
                Tenant = tenant
            };
            try
            {
                await _repo.AddUser(newUser);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost("new-User-Claim")]
        public async Task<IActionResult> AddUserClaimAsync(UClaims userClaims, string userId)
        {
            var user = _repo.FindById(userId);
            List<UserClaims> usrClaims = new List<UserClaims>();
            if (!String.IsNullOrEmpty(userClaims.Name))
            {
                var userClaimName = new UserClaims
                {
                    UserId = userId,
                    ClaimId = "15",
                    Value = userClaims.Name
                };
                usrClaims.Add(userClaimName);
            }
            if (!String.IsNullOrEmpty(userClaims.GivenName))
            {
                var userClaimGivenName = new UserClaims
                {
                    UserId = userId,
                    ClaimId = "13",
                    Value = userClaims.GivenName
                };
                usrClaims.Add(userClaimGivenName);
            }
            if (!String.IsNullOrEmpty(userClaims.FamilyName))
            {
                var userClaimFamilyName = new UserClaims
                {
                    UserId = userId,
                    ClaimId = "14",
                    Value = userClaims.FamilyName
                };
                usrClaims.Add(userClaimFamilyName);
            }
            if (!String.IsNullOrEmpty(userClaims.Email))
            {
                var userClaimEmail = new UserClaims
                {
                    UserId = userId,
                    ClaimId = "18",
                    Value = userClaims.Email
                };
                usrClaims.Add(userClaimEmail);
            }

            if (!String.IsNullOrEmpty(userClaims.Role))
            {
                var userClaimRole = new UserClaims
                {
                    UserId = userId,
                    ClaimId = "20",
                    Value = userClaims.Role
                };
                usrClaims.Add(userClaimRole);
            }
            if (!String.IsNullOrEmpty(userClaims.Tenant))
            {
                var userClaimTenant = new UserClaims
                {
                    UserId = userId,
                    ClaimId = "17",
                    Value = userClaims.Tenant
                };
                usrClaims.Add(userClaimTenant);
            }
            user.Claims = usrClaims;
            try
            {
                await _repo.UpdateUser(user);
                return Ok();
            }
            catch(Exception e)
            {
                return BadRequest();
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private void UpdateClientConfig(IS4Tenant tenant, List<int> serviceProviders)
        {
            foreach (var client in _configurationDbContext.Clients.AsQueryable().Where(c => serviceProviders.Contains(c.Id)).Include("RedirectUris").Include("PostLogoutRedirectUris"))
            {
                    client.RedirectUris.Add(new ClientRedirectUri() { Client = client, RedirectUri = $"https://{tenant.Name}.localhost:{(client.Id == 1 ? "44372" : "44334")}/signin-oidc-{tenant.TenantId}" });
                    client.PostLogoutRedirectUris.Add(new ClientPostLogoutRedirectUri() { Client = client, PostLogoutRedirectUri = $"https://{tenant.Name}.localhost:{(client.Id == 1 ? "44372" : "44334")}/signout-callback-oidc-{tenant.TenantId}" });
                    _configurationDbContext.Clients.Attach(client).State = EntityState.Modified;
            }
            _configurationDbContext.SaveChanges();
        }

    }
}
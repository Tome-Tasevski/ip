using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Data.Models;
using IdentityServerAspNetIdentity.Quickstart.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityServerAspNetIdentity.Quickstart.Admin
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
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost("add-role")]
        public async Task<IActionResult> AddRoleToUserAsync(string name)
        {
            try
            {
                await _repo.AddRole(name);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost("add-user")]
        public async Task<IActionResult> AddUserAsync(NewUser user, string tennat)
        {
            var tenant = _repo.GetTenant(tennat);
            var newUser = new ApplicationUser
            {
                UserName = user.UserName,
                Tenant = tenant,
                Email = user.Email
            };
            try
            {
                await _repo.AddUserAsync(newUser, user.Password);
                await _repo.AddClaimsToUser(newUser);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost("add-role-to-user")]
        public async Task<IActionResult> AddRoleToUserAsync(NewRoleToUser role, string userName)
        {
            try
            {
                await _repo.AddRoleToUser(userName, role.Name);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost("remove-users-role")]
        public async Task<IActionResult> RemoveUserRole(string userName, string role)
        {
            try
            {
                await _repo.RemoveUserRole(userName, role);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private void UpdateClientConfig(IS4Tenant tenant, List<int> serviceProviders)
        {
            foreach (var client in _configurationDbContext.Clients.AsQueryable().Where(c => serviceProviders.Contains(c.Id)).Include("RedirectUris").Include("PostLogoutRedirectUris"))
            {
                //this client.Id == 4 shoudl be dinamic !!!
                client.RedirectUris.Add(new ClientRedirectUri() { Client = client, RedirectUri = $"https://{tenant.Name}.localhost:{(client.Id == 4 ? "44372" : "44334")}/signin-oidc-{tenant.TenantId}" });
                client.PostLogoutRedirectUris.Add(new ClientPostLogoutRedirectUri() { Client = client, PostLogoutRedirectUri = $"https://{tenant.Name}.localhost:{(client.Id == 4 ? "44372" : "44334")}/signout-callback-oidc-{tenant.TenantId}" });
                _configurationDbContext.Clients.Attach(client).State = EntityState.Modified;
            }
            _configurationDbContext.SaveChanges();
        }

    }
}

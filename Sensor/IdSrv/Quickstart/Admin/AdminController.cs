﻿using System;
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
        public async Task<IActionResult> AddClient(NewClient client)
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
                UpdateClientConfig(newTenant);
                return Ok();
            }catch
            {
                return BadRequest();
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private void UpdateClientConfig(IS4Tenant tenant)
        {
            foreach (var client in _configurationDbContext.Clients.AsQueryable().Include("RedirectUris").Include("PostLogoutRedirectUris"))
            {
                client.RedirectUris.Add(new ClientRedirectUri() { Client = client, RedirectUri = $"https://{tenant.Name}.localhost:{(client.Id == 1 ? "44372" : "44334")}/signin-oidc-{tenant.TenantId}" });
                client.PostLogoutRedirectUris.Add(new ClientPostLogoutRedirectUri() { Client = client, PostLogoutRedirectUri = $"https://{tenant.Name}.localhost:{(client.Id == 1 ? "44372" : "44334")}/signout-callback-oidc-{tenant.TenantId}" });
                _configurationDbContext.Clients.Attach(client).State = EntityState.Modified;
            }
            _configurationDbContext.SaveChanges();
        }

    }
}
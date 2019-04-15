using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdSrv.Data;
using IdSrv.Data.Models;
using IdSrv.Quickstart.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdSrv.Quickstart.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly Repository _repo;
        public AdminController(Repository repo)
        {
            _repo = repo;
        }

        [HttpPost("new-client")]
        public IActionResult AddClient(NewClient client)
        {
            var newTenant = new IS4Tenant
            {
                Name = client.Name,
                LoginType = client.LoginType,
                Protocol = client.Protocol,
                TenantId = "4"
            };

            try
            {
                _repo.AddClient(newTenant);
                return Ok();
            }catch
            {
                return BadRequest();
            }
        }
        
    }
}
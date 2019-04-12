using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Quickstart.UI;
using IdentityServer4.Services;
using IdSrv.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdSrv.Quickstart.Configuration
{
    public class ConfigurationController : Controller
    {
        private readonly AuthSchemeProvider _authSchemeProvider;

        public ConfigurationController(AuthSchemeProvider authSchemeProvider)
        {
            _authSchemeProvider = authSchemeProvider;
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdate(string tenantId)
        {
            await _authSchemeProvider.AddOrUpdate(tenantId);
            return Redirect("/");
        }
    }
}
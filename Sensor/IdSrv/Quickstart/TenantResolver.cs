using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdSrv.Quickstart
{
    public class TenantResolver
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IIdentityServerInteractionService _interaction;

        public TenantResolver(IHttpContextAccessor contextAccessor,
            IIdentityServerInteractionService interaction)
        {
            _contextAccessor = contextAccessor;
            _interaction = interaction;
        }

        public string GetTenant()
        {
            var returnUrl = _contextAccessor.HttpContext.Features.Get<IHttpRequestFeature>().RawTarget;
            var authContext = _interaction.GetAuthorizationContextAsync(returnUrl).Result;

            return authContext.Tenant.Split(".").First();
        }
    }
}

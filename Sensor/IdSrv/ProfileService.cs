using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace IdSrv
{
    public class ProfileService : IProfileService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityServerInteractionService _interaction;

        public ProfileService(IHttpContextAccessor httpContextAccessor, IIdentityServerInteractionService interaction)
        {
            _httpContextAccessor = httpContextAccessor;
            _interaction = interaction;
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            context.IssuedClaims.AddRange(context.Subject.Claims);
            return Task.FromResult(0);
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            var features = _httpContextAccessor.HttpContext.Features.Get<IHttpRequestFeature>();
            var returnUrl = features.RawTarget;
            var authContext = _interaction.GetAuthorizationContextAsync(returnUrl).Result;

            var currentRequestTenant = authContext.Tenant.Split(".").First();
            var user_tenant = context.Subject.Claims.First(x => x.Type.Equals("tenant")).Value;
            if (!user_tenant.Equals(currentRequestTenant))
            {
                context.IsActive = false;
                
            }
            return Task.FromResult(context);
        }
    }
}

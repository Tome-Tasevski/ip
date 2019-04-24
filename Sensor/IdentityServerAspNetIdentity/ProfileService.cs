using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServerAspNetIdentity.Quickstart;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace IdentityServerAspNetIdentity
{
    public class ProfileService : IProfileService
    {
        private readonly TenantResolver _tenantResolver;

        public ProfileService(TenantResolver tenantResolver)
        {
            _tenantResolver = tenantResolver;
        }
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            context.IssuedClaims.AddRange(context.Subject.Claims);
            return Task.FromResult(0);
        }

        public Task IsActiveAsync(IsActiveContext context)
        {

            var currentRequestTenant = _tenantResolver.GetTenant();

            var user_tenant = context.Subject.Claims.First(x => x.Type.Equals("tenant")).Value;
            if (!user_tenant.Equals(currentRequestTenant))
            {
                context.IsActive = false;
            }
            return Task.FromResult(context);
        }
    }
}

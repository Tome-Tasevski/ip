using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace IDS4Admin
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
            var returnurl = features.RawTarget;
            var authcontext = _interaction.GetAuthorizationContextAsync(returnurl).Result;
            
            var currentrequesttenant = authcontext.Tenant.Split(".").First();
            var user_tenant = context.Subject.Claims.First(x => x.Type.Equals("tenant")).Value;
            if (!user_tenant.Equals(currentrequesttenant))
            {
                context.IsActive = false;

            }
            return Task.FromResult(context);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityExpress.Manager.BusinessLogic.Entities.Services;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4Admin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;

namespace IDS4Admin
{
    public class ProfileService : IProfileService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityServerInteractionService _interaction;
         private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileService(IHttpContextAccessor httpContextAccessor, 
            IIdentityServerInteractionService interaction,
             UserManager<ApplicationUser> userManager,
             SignInManager<ApplicationUser> signInManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _interaction = interaction;
            _userManager = userManager;
            _signInManager = signInManager;
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
          //  var identity = (ClaimsIdentity)User.Identity;
              //  First(x => x.Type.Equals("tenant")).Value;
           //IEnumerable<Claim> claims = identity.Claims;

           // var user_tenant = identity.FindFirst(x => x.Type.Equals("tenant")).Value;
           // if (!user_tenant.Equals(currentrequesttenant))
           // {
           //     context.IsActive = false;

           // }
            return Task.FromResult(context);
        }
    }
}

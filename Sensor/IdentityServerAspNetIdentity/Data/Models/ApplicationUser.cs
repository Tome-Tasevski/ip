
using Microsoft.AspNetCore.Identity;

namespace IdentityServerAspNetIdentity.Data.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public IS4Tenant Tenant { get; set; }
    }
}

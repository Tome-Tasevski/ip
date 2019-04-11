using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdSrv.Data.Models
{
    public class OpenIDConfig
    {
        [Key]
        public string OpenId { get; set; }
        public string SignInScheme { get; set; }
        public string SignOutScheme { get; set; }
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string GetClaimsFromUserInfoEndpoint { get; set; }

        public string TenantId { get; set; }
        public IS4Tenant Tenant { get; set; }

    }
}

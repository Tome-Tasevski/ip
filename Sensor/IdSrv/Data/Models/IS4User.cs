using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdSrv.Data.Models
{
    public class IS4User
    {
        [Key]
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string TenantId { get; set; }
        public IS4Tenant Tenant { get; set; }
        public string UserClaimsId { get; set; }
        public UserClaims UserClaims { get; set; }
        public ICollection<UserRole> Roles { get; set; }
    }
}

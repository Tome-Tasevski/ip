using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdSrv.Data.Models
{
    public class UserClaims
    {
        [Key]
        public string UserClaimsId { get; set; }
        public string Name { get; set; }
        public string GivenName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public ICollection<Role> Roles { get; set; }
    }
}

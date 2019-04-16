using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdSrv.Quickstart.DTOs
{
    public class UClaims
    {
        public string Name { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Tenant { get; set; }
    }
}

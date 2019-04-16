using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdSrv.Quickstart.DTOs
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string TenantId { get; set; }
        public string Provider { get; set; }
        public bool IsExternalUser { get; set; }
        public string ExternalUserId { get; set; }
    }
}

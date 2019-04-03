using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaaSMultiTenancy.Data
{
    public class AppTenant
    {
        public int AppTenantId { get; set; }
        public string Name { get; set; }
        public string Hostname { get; set; }
        public string Auth { get; set; }
    }
}

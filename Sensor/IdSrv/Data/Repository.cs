using IdSrv.Data.Context;
using IdSrv.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdSrv.Data
{
    public class Repository
    {
        private readonly IS4DbContext _dbContext;

        public Repository(IS4DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IS4Tenant GetTenant(string name)
        {
            return _dbContext.Set<IS4Tenant>().FirstOrDefault(x => x.Name.Equals(name));
        }
        
        public OpenIDConfig GetOpenIdConfig(string tenantId)
        {
            return _dbContext.Set<OpenIDConfig>().FirstOrDefault(x => x.TenantId.Equals(tenantId));
        }
    }
}

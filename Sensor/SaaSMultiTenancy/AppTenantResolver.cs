using Microsoft.AspNetCore.Http;
using SaasKit.Multitenancy;
using SaaSMultiTenancy.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaaSMultiTenancy
{
    public class AppTenantResolver : ITenantResolver<AppTenant>
    {
        private readonly TenantDbContext _dbContext;

        public AppTenantResolver(TenantDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public Task<TenantContext<AppTenant>> ResolveAsync(HttpContext context)
        {
            TenantContext<AppTenant> tenantContext = null;
            var hostName = context.Request.Host.Value.ToLower();

            var tenant = _dbContext.Tenants.FirstOrDefault(t => t.Hostname.Equals(hostName));

            if (tenant != null)
            {
                tenantContext = new TenantContext<AppTenant>(tenant);
            }

            return Task.FromResult(tenantContext);
        }
    }
}

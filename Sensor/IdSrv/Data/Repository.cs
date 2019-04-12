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

        public IQueryable<IS4Tenant> GetAllTenants()
        {
            return _dbContext.Set<IS4Tenant>();
        }

        public OpenIDConfig GetOpenIdConfig(string tenantId)
        {
            return _dbContext.Set<OpenIDConfig>().FirstOrDefault(x => x.TenantId.Equals(tenantId));
        }

        public SamlConfig GetSamlConfig(string tenantId)
        {
            return _dbContext.Set<SamlConfig>().FirstOrDefault(x => x.TenantId.Equals(tenantId));
        }

        public bool ValidateCredentials(string username, string password)
        {
            return _dbContext.Set<IS4User>().First(u => u.Username.Equals(username) && u.Password.Equals(password)) != null;
        }

        public IS4User FindByExternalProvider(string provider, string userId)
        {
            return _dbContext.Set<IS4User>().FirstOrDefault(u => u.Provider.Equals(provider) && u.ExternalUserId.Equals(userId));
        }

        public IS4User FindByUsername(string username)
        {
            return _dbContext.Set<IS4User>().FirstOrDefault(u => u.Username.Equals(username));
        }

        public void RegisterUser(IS4User user)
        {
            _dbContext.Add(user);
            _dbContext.SaveChanges();
        }
    }
}

using IdSrv.Data.Context;
using IdSrv.Data.Models;
using System.Collections.Generic;
using System.Linq;
using IdSrv.Quickstart.DTOs;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

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
            return _dbContext.Set<IS4User>().Include("Claims").Include("Claims").FirstOrDefault(u => u.Provider.Equals(provider) && u.ExternalUserId.Equals(userId));
        }

        public IS4User FindByUsername(string username)
        {
            var query = _dbContext.Set<IS4User>().AsQueryable();
            query = query.Include("Tenant").Where(u => u.Username == username);

            return query.FirstOrDefault();
        }

        public IS4User FindById(string userId)
        {
            return _dbContext.Set<IS4User>().Where(u => u.UserId == userId).FirstOrDefault();

        }

        public List<Role> GetRoles(string userId, bool isExternal)
        {
            var roles = new List<Role>();
            var query = _dbContext.Set<UserRole>().AsQueryable();

            if (isExternal)
            {
                query = query.Include("User").Include("Role").Where(u => u.User.ExternalUserId == userId);
            }
            else
            {
                query = query.Include("User").Include("Role").Where(u => u.User.UserId == userId);
            }

            foreach (var r in query)
            {
                roles.Add(r.Role);
            }

            return roles;
        }

        public void RegisterUser(IS4User user)
        {
            _dbContext.Add(user);
            _dbContext.SaveChanges();
        }

        public Role GetUserRole(string userId)
        {
            return _dbContext.Set<UserRole>().FirstOrDefault(x => x.User.UserId.Equals(userId)).Role;
        }

        public List<Claims> GetClaims()
        {
            return _dbContext.Set<Claims>().ToList();
        }

        public List<UserClaims> GetUserClaims(string userId, bool isExternal)
        {
            return _dbContext.Set<UserClaims>().Include("Claims").Where(x => !isExternal ? x.UserId.Equals(userId) : x.User.ExternalUserId.Equals(userId)).ToList();
        }

        internal void AddSamlConfig(SamlConfig samlcfg)
        {
            _dbContext.Add(samlcfg);
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Adds openidconfig in DB, needed for dynamic setup of oid schemes
        /// </summary>
        /// <param name="cfg"></param>
        public void AddOIDConfig(OpenIDConfig cfg)
        {
            _dbContext.Add(cfg);
            _dbContext.SaveChanges();
        }

        public async Task AddClient(IS4Tenant tenant)
        {
            _dbContext.Add(tenant);
            await _dbContext.SaveChangesAsync();
        }

        public IS4Tenant GetTenantById(string tenantId)
        {
            return _dbContext.Set<IS4Tenant>().FirstOrDefault(x => x.TenantId.Equals(tenantId));
        }
        public async Task AddUser(IS4User user)
        {
            _dbContext.Add(user);
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdateUser(IS4User user)
        {
            _dbContext.Attach(user).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

    }
}

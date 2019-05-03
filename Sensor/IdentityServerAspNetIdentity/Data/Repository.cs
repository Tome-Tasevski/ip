using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServerAspNetIdentity.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityServerAspNetIdentity.Data
{
    public class Repository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public Repository(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
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

        //public bool ValidateCredentials(string username, string password)
        //{
        //    return _dbContext.Set<IS4User>().First(u => u.Username.Equals(username) && u.Password.Equals(password)) != null;
        //}

        public ApplicationUser FindByExternalProvider(string provider, string userId)
        {
            //da se proveri vo AspNetUserLogins, LoginProvider e facebook primer
            //return _dbContext.Set<ApplicationUser>().Include("Claims").Include("Claims").FirstOrDefault(u => u.Provider.Equals(provider) && u.ExternalUserId.Equals(userId));
            return _dbContext.Set<ApplicationUser>().FirstOrDefault(u => u.Id.Equals(userId) );//ova ne e ok treba da se doraboti
        }

        public ApplicationUser FindByUsername(string username)
        {
            var query = _dbContext.Set<ApplicationUser>().AsQueryable();
            query = query.Include("Tenant").Where(u => u.UserName == username);

            return query.FirstOrDefault();
        }

        public ApplicationUser FindById(string userId)
        {
            return _dbContext.Set<ApplicationUser>().Where(u => u.Id == userId).FirstOrDefault();

        }
        public void RegisterUser(ApplicationUser user)
        {
            _dbContext.Add(user);
            _dbContext.SaveChanges();
        }

      //moze kje treba d ase dodadat logiki za caims

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

        public async Task AddUserAsync(ApplicationUser user, string password)
        {
            await _userManager.CreateAsync(user, password);
        }

        public async Task AddRoleToUser(string userName, string role)
        {
            var user = await _userManager.FindByNameAsync(userName);
            await _userManager.AddToRoleAsync(user, role);
            await _userManager.AddClaimAsync(user, new Claim(JwtClaimTypes.Role, role));
        }

        public async Task RemoveUserRole(string userName, string role)
        {
            var user = await _userManager.FindByNameAsync(userName);
            await _userManager.RemoveFromRoleAsync(user, role);
            await _userManager.RemoveClaimAsync(user, new Claim(JwtClaimTypes.Role, role));
        }

        public async Task AddClaimsToUser(ApplicationUser user)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(JwtClaimTypes.Name, user.UserName));
            claims.Add(new Claim(JwtClaimTypes.Email, user.Email));
            claims.Add(new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean));
            claims.Add(new Claim("tenant", user.Tenant.Name));

            await _userManager.AddClaimsAsync(user, claims);
        }

        public async Task AddRole(string name)
        {
            var role = new IdentityRole();
            role.Name = name;
            await _roleManager.CreateAsync(role);
        }
    }
}

using IdSrv.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdSrv.Data.Context
{
    public class IS4DbContext : DbContext
    {
        public IS4DbContext(DbContextOptions<IS4DbContext> options)
            : base(options)
        {

        }

        DbSet<IS4Tenant> Tenants { get; set; }
        DbSet<IS4User> Users { get; set; }
        DbSet<OpenIDConfig> OpenIDConfigs { get; set; }
        DbSet<SamlConfig> SamlConfigs { get; set; }
    }
}

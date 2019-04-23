using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerAspNetIdentity.Models
{
    public class IS4Tenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string TenantId { get; set; }
        public string Name { get; set; }
        public string LoginType { get; set; }
        public string Protocol { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }
    }
}

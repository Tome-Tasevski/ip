using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerAspNetIdentity.Models
{
    public class OpenIDConfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string OpenId { get; set; }
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public string TenantId { get; set; }
        public IS4Tenant Tenant { get; set; }
    }
}

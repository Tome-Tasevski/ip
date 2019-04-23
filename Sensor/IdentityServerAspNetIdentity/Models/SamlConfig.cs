using IdentityServer4.Saml.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerAspNetIdentity.Models
{
    public class SamlConfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string SamlId { get; set; }
        public string IdpEntityId { get; set; }
        public string IdpSigningCertificate { get; set; }
        public string SingleSignOnEndpoint { get; set; }
        public string SingleLogoutEndpoint { get; set; }

        public string TenantId { get; set; }
        public IS4Tenant Tenant { get; set; }
    }
}

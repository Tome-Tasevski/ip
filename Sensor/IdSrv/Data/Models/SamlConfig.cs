using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdSrv.Data.Models
{
    public class SamlConfig
    {
        [Key]
        public string SamlId { get; set; }
        public string Licensee { get; set; }
        public string LicenseKey { get; set; }
        public string IdpEntityId { get; set; }
        public string IdpSigningCertificate { get; set; }
        public string SingleSignOnEndpoint { get; set; }
        public string SingleLogoutEndpoint { get; set; }
        public string SpEntityId { get; set; }
        public string MetadataPath { get; set; }
        public string SignAuthenticationRequests { get; set; }
        public string SpSigningCertificate { get; set; }
        public bool SaveTokens { get; set; }
        public string NameIdClaimType { get; set; }
        public string SignInScheme { get; set; }
        public int TimeComparisonTolerance { get; set; }

        public string TenantId { get; set; }
        public IS4Tenant Tenant { get; set; }
    }
}

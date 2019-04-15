using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdSrv.Data.Models
{
    public class IS4User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsExternalUser { get; set; }
        public string Provider { get; set; }
        public string ExternalUserId { get; set; }

        [NotMapped]
        public List<Claim> Claims { get; set; }

        public string TenantId { get; set; }
        public IS4Tenant Tenant { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }
}

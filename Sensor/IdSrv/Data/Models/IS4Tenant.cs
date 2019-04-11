using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdSrv.Data.Models
{
    public class IS4Tenant
    {
        [Key]
        public string TenantId { get; set; }
        public string Name { get; set; }
        public string LoginType { get; set; }
        public string Protocol { get; set; }
        
        public ICollection<IS4User> Users { get; set; }
    }
}

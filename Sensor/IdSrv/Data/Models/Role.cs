using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdSrv.Data.Models
{
    public class Role
    {
        [Key]
        public string RoleId { get; set; }
        public string Name { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
    }
}

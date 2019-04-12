using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IdSrv.Data.Models
{
    public class UserRole
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string UserRoleId { get; set; }
        public string RoleId { get; set; }
        public Role Role { get; set; }
        public string UserId { get; set; }
        public IS4User User { get; set; }
    }
}

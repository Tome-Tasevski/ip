using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IdSrv.Data.Models
{
    public class UserClaims
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public IS4User User { get; set; }
        public string UserId { get; set; }
        public Claims Claims { get; set; }
        public string ClaimId { get; set; }
        public string Value { get; set; }
    }
}

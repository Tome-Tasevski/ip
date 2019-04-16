using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdSrv.Quickstart.DTOs
{
    public class SMLConfig
    {
        public string IdpEntityId { get; set; }
        public string IdpSigningCertificate { get; set; }
        public string SSOEndpoint { get; set; }
        public string SLOEndpoint { get; set; }
        
    }
}

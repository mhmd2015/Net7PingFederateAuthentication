using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterBlazor.Authentication
{
    public class AuthenticationOption
    {
        public string Issuer { get; set; }
        public string Wtrealm { get; set; }
        public string ValidAudience { get; set; }
        public string MetadataAddress { get; set; }
        public string ValidIssuer { get; set; }
        public string CertificatePath { get; set; }

    }
}

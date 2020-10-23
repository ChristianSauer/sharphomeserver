using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharpHomeServer
{
    public class RavenDbOptions
    {
        public static string RavenDb => "RavenDb";

        /// <summary>
        /// todo sanity checks
        /// </summary>
        public string Database { get; set; }

        public string[] Urls { get; set; }

        public string CertificatePath { get; set; }

        public string Password { get; set; }
    }
}

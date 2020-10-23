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
<<<<<<< HEAD

        public string Password { get; set; }
=======
>>>>>>> da0686ff14f3d9073ed1bf1e7eacbc242f1abb0b
    }
}

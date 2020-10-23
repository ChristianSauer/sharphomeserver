using Microsoft.Extensions.Options;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SharpHomeServer
{
    public class RavenDbDocumentStore : IRavenDbDocumentStore
    {

        public RavenDbDocumentStore(IOptions<RavenDbOptions> options)
        {
            ravenDbOptions = options.Value;
            store = new Lazy<IDocumentStore>(CreateStore, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private readonly RavenDbOptions ravenDbOptions;

        // Use Lazy<IDocumentStore> to initialize the document store lazily. 
        // This ensures that it is created only once - when first accessing the public `Store` property.
        private readonly Lazy<IDocumentStore> store;

        public IDocumentStore Store => store.Value;

        private IDocumentStore CreateStore()
        {
            IDocumentStore store = new DocumentStore()
            {
                // Define the cluster node URLs (required)
                Urls = ravenDbOptions.Urls,

                // Define a default database (optional)
                Database = ravenDbOptions.Database,

                // Define a client certificate (optional)
                Certificate = new X509Certificate2(ravenDbOptions.CertificatePath, "phi1plun-FEA5lont"),

                // Initialize the Document Store
            }.Initialize();

            return store;
        }
    }
}

using Microsoft.Extensions.Hosting;
using Raven.Client.Documents.Indexes;
using System.Threading;
using System.Threading.Tasks;

namespace SharpHomeServer
{
    public class IndexDeployer : BackgroundService
    {
        private readonly IRavenDbDocumentStore store;

        public IndexDeployer(IRavenDbDocumentStore store)
        {
            this.store = store;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await IndexCreation.CreateIndexesAsync(typeof(IndexDeployer).Assembly, store.Store, token: stoppingToken);
        }
    }
}

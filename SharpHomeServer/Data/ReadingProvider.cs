using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;

namespace SharpHomeServer.Data
{
    public class ReadingProvider : IReadingProvider
    {
        private readonly IRavenDbDocumentStore store;

        public ReadingProvider(IRavenDbDocumentStore store)
        {
            this.store = store;
        }

        public async Task<List<string>> GetReadingTypesAsync()
        {

            using var session = store.Store.OpenAsyncSession();
            var result = await session.Query<Reading>(collectionName: "GasReadings") // todo GasReadings collection has to be renamed, needs changing the python client
            .OrderBy(x => x.reading_type)
            .Select(x => x.reading_type)
            .Distinct()
            .ToListAsync();

            return result;
        }
    }
}

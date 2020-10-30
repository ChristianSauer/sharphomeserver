using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plotly.Blazor.Traces.IsoSurfaceLib.CapsLib;
using Raven.Client.Documents;
using Raven.Client.Documents.Queries;

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
            var result = await session.Query<Reading>(collectionName: "Datas")
            .OrderBy(x => x.readingType)
            .Select(x => x.readingType)
            .Distinct()
            .ToListAsync();

            return result;
        }

        public (List<DateTime>, List<double>) GetReadingTimeSeries(string type)
        {
            using var session = store.Store.OpenSession();
            var query = session.Query<Reading>(collectionName: "Datas").Where(x => x.readingType == type).Select(q => RavenQuery.TimeSeries(q, "'gas m3 usage'") // todo parametrize
       .GroupBy(g => g.Days(1))

       .Select(g => new
       {
           Min = g.Min(),
           Max = g.Max()
       }).ToList());

            var result = query.ToList();

            IEnumerable<double> enumerable = result.FirstOrDefault().Results.Select(x => x.Max.First() - x.Min.First());
            return (result.FirstOrDefault().Results.Select(x => x.From).ToList(), enumerable.ToList());
        }

        public (List<DateTime>, List<double>) GetGasCost(string type)
        {
            (var x, var y) = GetReadingTimeSeries(type);

            var zNumber = 0.9563; // todo config
            var brennwert = 10.2840;  // todo config and this varies ofer the years

            var costPerKwh = 4.83; // todo config

            var kwh = y.Select(x => x * zNumber * brennwert);  //kwh

            var euros = kwh.Select(x => x * costPerKwh / 100);

            return (x, euros.ToList());
        }

    }
}

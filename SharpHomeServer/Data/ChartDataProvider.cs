using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raven.Client.Documents.Operations.TimeSeries;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Queries.TimeSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharpHomeServer.Data
{
    public class ChartDataProvider : IChartDataProvider
    {
        private readonly IRavenDbDocumentStore store;
        private readonly ILogger<ChartDataProvider> logger;

        public ChartDataProvider(IOptions<ChartOptions> options, IRavenDbDocumentStore store, ILogger<ChartDataProvider> logger)
        {
            if (options.Value is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Charts = options.Value;

            this.store = store;
            this.logger = logger;
        }

        public async Task<(DateTime, DateTime)> GetTimeSeriesDatetimeRange(string document, string timeSeries)
         {

            var stats = await store.Store.Operations.SendAsync(new GetTimeSeriesStatisticsOperation(document));

            var ts = stats.TimeSeries.
                FirstOrDefault(x => x.Name == timeSeries);

            if (ts == null)
            {
                throw new ArgumentOutOfRangeException($"the timeseries {timeSeries} does not exist in the document {document}");
            }

            return (ts.StartDate, ts.EndDate);
        }

        public (string collection, string docId) SplitDocumentName(string document)
        {

            var splitted = document.Split("/");

            if (splitted.Length != 2)
            {
                throw new ArgumentOutOfRangeException($"document '{document}' should have the format 'collection/document'");
            }

            var collection = splitted[0] + "s";
            var docId = splitted[1];

            return (collection, docId);
        }

        public (List<DateTime>, List<double>) GetReadingTimeSeries(string document, string timeSeries, string groupBy, DateTime start, DateTime end)
        {
            (string collection, string docId) = SplitDocumentName(document);
            using var session = store.Store.OpenSession();

            string groupByUnit = $"1 {groupBy}";

            logger.LogDebug("querying {document}:{timeseries} from '{start}' to '{end}' grouped by {groupBy}", document, timeSeries, start, end, groupBy);

            var query = session.Query<Reading>(collectionName: collection)
                .Where(x => x.readingType == docId)
                .Select(q => RavenQuery
                    .TimeSeries(q, timeSeries, start, end)
                    .GroupBy(groupByUnit)
                    .Select(g => new
                    {
                        Min = g.Min(),
                        Max = g.Max(),
                        Average = g.Average()
                    })
                    .ToList());

            var result = query.ToList();
            Func<TimeSeriesRangeAggregation, double> aggregationFunc = getAggregationFunc(document, timeSeries);
            TimeSeriesRangeAggregation[] data = result.FirstOrDefault().Results;
            if (data == null)
            {
                logger.LogWarning($"No data for {document}:{timeSeries} return empty list");
                return (new List<DateTime>(), new List<double>());
            }

            var y_values = data.Select(aggregationFunc).ToList();
            var x_values = data.Select(x => x.From).ToList();

            logger.LogDebug("found {count} values", x_values.Count);

            return (x_values, y_values);
        }

        private Func<TimeSeriesRangeAggregation, double> getAggregationFunc(string document, string timeSeries)
        {
            var options = GetChartOptionFor(document, timeSeries);
            Func<TimeSeriesRangeAggregation, double> aggregationFunc = (options.AggregationMode.ToLowerInvariant()) switch
            {
                "max-min" => (x) => x.Max.First() - x.Min.First(),
                "max" => (x) => x.Max.First(),
                "min" => (x) => x.Min.First(),
                "average" => (x) => x.Average.First(),
                _ => throw new ArgumentException($"Unknown Aggregation Mode: {options.AggregationMode}"),
            };
            return aggregationFunc;
        }

        public ChartOption GetChartOptionFor(string document, string timeSeries)
        {
            return Charts.Charts.
                First(x => x.DocumentId == document && x.TimeSeries == timeSeries);
        }

        public ChartOptions Charts { get; }
    }
}

// Start > End verhindern
// Deep linking
// todo set chart to hidden while loading
// todo set better yaxis -> datetime
// todo use async for ravendb usage?
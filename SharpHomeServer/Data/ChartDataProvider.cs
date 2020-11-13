using Microsoft.Extensions.Options;
using Raven.Client.Documents.Operations.TimeSeries;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Queries.TimeSeries;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SharpHomeServer.Data
{
    public class ChartDataProvider : IChartDataProvider
    {
        private IRavenDbDocumentStore store;

        public ChartDataProvider(IOptions<ChartOptions> options, IRavenDbDocumentStore store)
        {
            if (options.Value is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Charts = options.Value;

            this.store = store;
        }

        public async Task<(DateTime, DateTime)> GetTimeSeriesDatetimeRange(string document, string timeSeries)
         {

            var stats = await store.Store.Operations.SendAsync(new GetTimeSeriesStatisticsOperation(document));

            var ts = stats.TimeSeries.Where(x => x.Name == timeSeries).FirstOrDefault();

            if (ts == null)
            {
                throw new ArgumentOutOfRangeException($"the timeseries {timeSeries} does not exist in the document {document}");
            }

            return (ts.StartDate, ts.EndDate);
        }

        public (List<DateTime>, List<double>) GetReadingTimeSeries(string document, string timeSeries, string groupBy, DateTime start, DateTime end)

        {
            var splitted = document.Split("/");
            // todo error if  =!2

            var collection = splitted[0] + "s"; // todo check?
            var docId = splitted[1];

            var escaped = $"'{timeSeries}'"; // workaround fur current ravendb bug with timeseries names with blanks

            using var session = store.Store.OpenSession();

            string groupByUnit = $"1 {groupBy}";

            ValidGroupByTimes validGroupBy =  Enum.Parse<ValidGroupByTimes>(groupBy);

            var query = session.Query<Reading>(collectionName: collection)
                .Where(x => x.readingType == docId)
                .Select(q => RavenQuery
                    .TimeSeries(q, escaped, start, end)
                    .GroupBy(groupByUnit)
                    .Select(g => new
                    {
                        Min = g.Min(),
                        Max = g.Max(),
                        Average = g.Average()
                    })
                    .ToList());
            var x = query.Expression;
            var result = query.ToList();

            var options = GetChartOptionFor(document, timeSeries);
            Func<TimeSeriesRangeAggregation, double> aggregationFunc = (options.AggregationMode.ToLowerInvariant()) switch
            {
                "max-min" => (x) => x.Max.First() - x.Min.First(),
                "max" => (x) => x.Max.First(),
                "min" => (x) => x.Min.First(),
                "average" => (x) => x.Average.First(),
                _ => throw new ArgumentException($"Unknown Aggregation Mode: {options.AggregationMode}"),
            };

            TimeSeriesRangeAggregation[] data = result.FirstOrDefault().Results;
            var y_values = data.Select(aggregationFunc).ToList();
            var x_values = data.Select(x => x.From).ToList();
            return (x_values, y_values);
        }

        public ChartOption GetChartOptionFor(string document, string timeSeries)
        {
            return Charts.Charts.Where(x => x.DocumentId == document && x.TimeSeries == timeSeries).First();
        }

        public ChartOptions Charts { get; }
    }
}

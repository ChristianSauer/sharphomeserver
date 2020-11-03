using Raven.Client.Documents.Indexes.TimeSeries;

namespace SharpHomeServer
{
    /// <summary>
    /// This index is responsible for getting the start / end time of all timeseries we find in the collection "datas"
    /// </summary>
    public class Index_tsRange : AbstractTimeSeriesIndexCreationTask
    {
        public override string IndexName => "tsRange";

        public override TimeSeriesIndexDefinition CreateIndexDefinition()
        {
            return new TimeSeriesIndexDefinition
            {
                Maps =
            {
            @"from ts in timeSeries.datas
select new {
    ts.Start,
    ts.End,
    ts.DocumentId,
    ts.Name
}"
            },
                Reduce = @"from result in results
group result by new { result.DocumentId, result.Name } into g
select new {
   DocumentId = g.Key.DocumentId,
   Name = g.Key.Name,
   Start = g.Min(x => x.Start),
   End = g.Max(x => x.End)
}"
            };
        }
    }
}

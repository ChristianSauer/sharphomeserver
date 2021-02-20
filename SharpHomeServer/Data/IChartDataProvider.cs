using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpHomeServer.Data
{
    public interface IChartDataProvider
    {
        ChartOptions Charts { get; }

        (List<DateTime> x_values, List<double> y_values) GetReadingTimeSeries(string document, string timeSeries, string groupBy, DateTime start, DateTime end);

        ChartOption GetChartOptionFor(string document, string timeSeries);

        Task<(DateTime, DateTime)> GetTimeSeriesDatetimeRange(string document, string timeSeries);
    }
}
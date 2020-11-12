using System;
using System.Collections.Generic;

namespace SharpHomeServer.Data
{
    public interface IChartDataProvider
    {
        ChartOptions Charts { get; }

        (List<DateTime>, List<double>) GetReadingTimeSeries(string document, string timeSeries, string groupBy);

        ChartOption GetChartOptionFor(string document, string timeSeries);
    }
}
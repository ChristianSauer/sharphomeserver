using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharpHomeServer
{
    public class ChartDataProvider : IChartDataProvider
    {

        public ChartDataProvider(IOptions<ChartOptions> options)
        {
            Charts = options.Value;
        }

        public ChartOptions Charts { get; }
    }
}

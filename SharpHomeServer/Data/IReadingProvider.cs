using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpHomeServer.Data
{
    public interface IReadingProvider
    {
        Task<List<string>> GetReadingTypesAsync();

        (List<DateTime>, List<double>) GetReadingTimeSeries(string type);

        (List<DateTime>, List<double>) GetGasCost(string type);
    }
}
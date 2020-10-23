using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpHomeServer.Data
{
    public interface IReadingProvider
    {
        Task<List<string>> GetReadingTypesAsync();
    }
}
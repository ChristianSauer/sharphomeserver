using System.Collections.Generic;

namespace SharpHomeServer.Data
{
    public class TimeSelector 
    {
        public static readonly IReadOnlyList<string> ValidGroupByTimes = new List<string>() { "Minutes", "Hours", "Days", "Months", "Quarters", "Years" };
    }

    public enum ValidGroupByTimes
    {
        Days,
        Hours,
        Milliseconds,
        Minutes,
        Months,
        Quarters,
        Seconds,
        Years,
    }
}

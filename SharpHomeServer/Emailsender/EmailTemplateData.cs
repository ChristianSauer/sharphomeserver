using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharpHomeServer.EmailSender
{
    public class EmailTemplateData
    {
        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public IEnumerable<Entry> Entries { get; set; }
    }
}

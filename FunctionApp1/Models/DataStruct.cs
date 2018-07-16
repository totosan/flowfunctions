using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowFunctionsTT
{
    public class DataStruct
    {
        public Guid CorrellationId { get; set; }

        public int Month { get; set; }

        public string MonthName { get; set; }

        public List<List<string>> Rows { get; set; }

        public DateTime TimeStamp { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowFunctionsTT.Models
{
    public class Projektprovision
    {
        public string Projektname { get; set; }

        public List<Tagesprovision> Tagesprovisionen { get; set; }

        public List<Taskprovision> Taskprovisionen { get; set; }
    }

    public class Taskprovision
    {
        public string Name { get; set; }

        public string Tagessatz { get; set; }

        public List<Tagesprovision> Tagesprovisionen { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FlowFunctionsTT.Models
{
    public class ElementValue
    {
        public XElement Element { get; set; }

        public int DistanceFromRoot { get; set; }
    }
}

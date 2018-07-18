using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowFunctionsTT.Models
{
    public class AccountProvisionsdaten
    {
        public string Displayname { get; set; }

        public string Name { get; set; }

        public string PersonalNummer { get; set; }

        public string Provisionssatz { get; set; }

        public string Eigenprovision { get; set; }

        public string Teamprovision { get; set; }

        public string Gesamtprovision { get; set; }

        public List<Tagesprovision> Tagesprovisionen { get; set; }

        public List<Projektprovision> Projektprovisionen { get; set; }
    }
}

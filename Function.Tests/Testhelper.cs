using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FlowFunctionsTT.Models;

namespace Function.Tests
{
    public class Testhelper
    {
        public static XDocument GetXml()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("Function.Tests.Provisionsanspruch-Teamleiter.rdl.xml");
            new HandleXml();
            var streamReader = new StreamReader(stream);
            var xmlStuff = HandleXml.GetTeamMemberProvisionDetails(streamReader);
            streamReader.Dispose();
            stream.Dispose();

            return xmlStuff;
        }
    }
}

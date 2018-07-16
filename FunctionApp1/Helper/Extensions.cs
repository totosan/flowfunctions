using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace FlowFunctionsTT.Helper
{
    public static class Extensions
    {
        public static bool IsNearestParent(this XElement element, string name)
        {
            if (element == null)
            {
                return false;
            }

            XElement parent = element.Parent;
            while (parent != null && parent.Name.LocalName.Contains(name))
            {
                parent = parent.Parent;
            }

            return parent == null;
        }

        public static int NumberOfParents(this XElement element)
        {
            int i = 0;
            XElement parent = element.Parent;
            while (parent != null)
            {
                i++;
                parent = parent.Parent;
            }

            return i;
        }

        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using (var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }
    }
}

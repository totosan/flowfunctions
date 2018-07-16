using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using FlowFunctionsTT.Helper;

namespace FlowFunctionsTT.Models
{
    public class HandleXml
    {
        private static XmlNamespaceManager _namespaceManager = new XmlNamespaceManager(new NameTable());

        public HandleXml()
        {
            _namespaceManager.AddNamespace("dft", "Provisionsanspruch-Teamleiter.rdl");
        }

        public static XDocument GetTeamMemberProvisionDetails(StreamReader reader)
        {
            var xml = XDocument.Load(reader);
            var accounts = xml.Descendants().FirstOrDefault(elem => elem.Name.LocalName.Contains("ResourceNTAccount_Collection"));
            return new XDocument(accounts);
        }

        public static List<AccountProvisionsdaten> GetStrippedAccountDetails(XDocument document)
        {
            document.Descendants()
                .Attributes()
                .Where(x => x.IsNamespaceDeclaration)
                .Remove();

            foreach (var elem in document.Descendants())
                elem.Name = elem.Name.LocalName;

            var accounts = document.Descendants(XName.Get("ResourceNTAccount"));
            var accountDetails = new List<AccountProvisionsdaten>();

            foreach (var account in accounts.Cast<XElement>())
            {
                var accountDetail = new AccountProvisionsdaten();

                var props = account
                    .Descendants()
                    .Where(elem => elem.HasAttributes);

                GetAccountDetails(accountDetail, props);
                accountDetail.Tagesprovisionen = GetByDateDetails(document);
                accountDetail.Projektprovisionen = GetProjectDetails(document);
                accountDetails.Add(accountDetail);
            }

            return accountDetails;
        }

        private static void GetAccountDetails(AccountProvisionsdaten accountDetail, IEnumerable<XElement> props)
        {
            accountDetail.Provisionssatz = GetByAttribName(props, "Provisionssatz").FirstOrDefault().Element?.LastAttribute.Value;

            accountDetail.Displayname = GetByAttribName(props, "ResourceName").FirstOrDefault().Element?.LastAttribute.Value;

            accountDetail.Eigenprovision = GetByAttribName(props, "ProjektundBonusProvision")
                .OrderBy(elem => elem.DistanceFromRoot)
                .FirstOrDefault()
                .Element?.LastAttribute.Value;

            accountDetail.Teamprovision = GetByAttribName(props, "Textbox")
                .OrderBy(elem => elem.DistanceFromRoot)
                .FirstOrDefault()
                .Element?.LastAttribute.Value;

            accountDetail.Gesamtprovision = GetByAttribName(props, "Textbox")
                .OrderBy(elem => elem.DistanceFromRoot)
                .Skip(1)
                .FirstOrDefault()
                .Element?.LastAttribute.Value;
        }

        public static List<Projektprovision> GetProjectDetails(XDocument xmlDoc)
        {
            var projects = xmlDoc.Descendants().Where(el => el.Name.LocalName == "ProjectName");

            var projectDetails = new List<Projektprovision>();
            foreach (var project in projects)
            {
                var projectdetail = new Projektprovision();

                var projName = project.Descendants()
                    .FirstOrDefault(el => el.HasAttributes && el.FirstAttribute.Name.LocalName == "ProjectName");
                projectdetail.Projektname = projName?.FirstAttribute.Value;

                var yearCollection = HandleXml.GetByDateDetails(new XDocument(project));

                projectdetail.Tagesprovisionen = yearCollection;

                var tasks = project.Descendants().Where(el => el.Name.LocalName == "TaskName");

                projectdetail.Taskprovisionen = new List<Taskprovision>();

                foreach (var task in tasks)
                {
                    var taskDetail = new Taskprovision();

                    var taskName = task.Descendants()
                        .FirstOrDefault(el => el.HasAttributes && el.FirstAttribute.Name.LocalName == "TaskName");
                    taskDetail.Name = taskName?.FirstAttribute.Value;

                    taskDetail.Tagessatz = task.Descendants()
                        .FirstOrDefault(el => el.HasAttributes && el.FirstAttribute.Name.LocalName == "Tagessatz_A")?
                        .FirstAttribute.Value;

                    taskDetail.Tagesprovisionen = HandleXml.GetByDateDetails(new XDocument(task));
                    projectdetail.Taskprovisionen.Add(taskDetail);
                }

                projectDetails.Add(projectdetail);
            }

            return projectDetails;
        }


        public static List<Tagesprovision> GetByDateDetails(XDocument xmlDoc)
        {
            var yearElement = xmlDoc.Descendants().FirstOrDefault(el => el.Name.LocalName == ("Jahre"));
            var monthElement = yearElement.Descendants().FirstOrDefault(el => el.Name.LocalName == "Monate");
            var days = monthElement.Descendants().Where(el => el.Name.LocalName == "Tage");

            var dayDetails = new List<Tagesprovision>();
            foreach (var day in days)
            {
                var dayDetail = new Tagesprovision();
                var date1 = $"{yearElement.FirstAttribute.Value}-{monthElement.FirstAttribute.Value.GetFromMonthName()}-{day.Elements().FirstOrDefault()?.FirstAttribute.Value.Trim('.')}";

                var valuesInDay = day.Elements().FirstOrDefault()?.Elements().ToList();

                dayDetail.Arbeit = valuesInDay?[0].FirstAttribute?.Value;
                dayDetail.Umsatz = valuesInDay?[1].FirstAttribute?.Value;
                dayDetail.Eigenprovision = valuesInDay?[2].FirstAttribute?.Value;
                dayDetail.Teamprovision = valuesInDay?[3].FirstAttribute?.Value;
                dayDetail.Datum = DateTime.Parse(date1);
                dayDetails.Add(dayDetail);
            }

            return dayDetails;
        }

        private static List<ElementValue> GetByAttribName(IEnumerable<XElement> props, string attributeName)
        {
            var elements = props.Where(elem => elem.LastAttribute.Name.LocalName.Contains(attributeName));
            return elements.Select(elem => new ElementValue { Element = elem, DistanceFromRoot = elem.NumberOfParents() }).ToList();
        }

        private static IEnumerable<XElement> GetElementsByElementsName(XElement parentElement, string name)
        {
            return parentElement.Descendants().Where(elem => elem.Name.LocalName.Contains(name));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using FlowFunctionsTT;
using FlowFunctionsTT.Helper;
using FlowFunctionsTT.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Function.Tests
{
    [TestClass]
    public class FunctionTests
    {
        [TestMethod]
        public void Can_strip_xml()
        {
            var xmlStuff = Testhelper.GetXml();

            Assert.IsNotNull(xmlStuff);
            Assert.IsTrue(xmlStuff.Root.Name.LocalName.Contains("ResourceNTAccount_Collection"));

        }

        [TestMethod]
        public void Can_get_only_relevant_account_data()
        {
            var xmlStuff = Testhelper.GetXml();
            var accountDetails = HandleXml.GetStrippedAccountDetails(xmlStuff);

            Assert.IsNotNull(accountDetails);
            Assert.IsTrue(accountDetails.Count() == 5);
        }

        [TestMethod]
        public void Can_get_day_details()
        {
            var xmlDoc = Testhelper.GetXml();

            List<Tagesprovision> dayDetails = HandleXml.GetByDateDetails(xmlDoc);
            Assert.IsTrue(dayDetails.Count == 21);
            Assert.IsTrue(dayDetails[0].Arbeit == "8.000000");
            Assert.IsTrue(dayDetails[0].Umsatz == "900");
            Assert.IsTrue(dayDetails[0].Eigenprovision == "90");
            Assert.IsTrue(dayDetails[0].Teamprovision == "1821.643125");
        }

        [TestMethod]
        public void Can_get_project_details()
        {
            var xmlDoc = Testhelper.GetXml();

            List<Projektprovision> projectDetails = HandleXml.GetProjectDetails(xmlDoc);

            Assert.IsTrue(projectDetails[0].Projektname.Contains("17.003.02.10614"));
            Assert.IsTrue(projectDetails[0].Tagesprovisionen.Count == 21);
            Assert.IsTrue(projectDetails[0].Taskprovisionen.Count == 2);
            Assert.IsTrue(projectDetails[0].Taskprovisionen[0].Tagesprovisionen.Count == 21);
        }
    }
}

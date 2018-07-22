using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using FlowFunctionsTT.Helper;
using FlowFunctionsTT.Models;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowFunctionsTT
{

    public static class ProvisionReportImporter
    {
        static readonly string _uriTemplate = "https://pm.alegri.eu/_vti_bin/ReportServer?https%3a%2f%2fpm.alegri.eu%2freports%2fProject+Server+Reports%2fProvisionsanspruch-Teamleiter.rdl&Start={0}%2F{1}%2F{2}%2000%3A00%3A00&Ende={0}%2F{3}%2F{2}%2000%3A00%3A00&Scope=2&Lokation=TOP.10000&Lokation=TOP.12200&Lokation=TOP.20000&Lokation=TOP.40000&Lokation=TOP.50000&Lokation=TOP.60000&Lokation=TOP.70000&Teamleiter=TOP.20000.21000.21004&Mitarbeiter=c9e2a315-fe00-e611-943f-00155d595825&Mitarbeiter=1d01683a-10da-e711-944d-00155d595824&Mitarbeiter=19b41e5f-5ef8-e511-9442-00155dc80320&Mitarbeiter=1e01683a-10da-e711-944d-00155d595824&Mitarbeiter=a636e87e-b080-454f-9a86-f0de390524b5&Projektart=Intern%20Bonusprogramm&Projektart=Intern%20Organisation&Projektart=Intern%20PreSales&Projektart=Intern%20Verwaltung&Projektart=Kundenprojekt%20Dienstvertrag%20TimeandMaterial&Projektart=Kundenprojekt%20Werkvertrag%20Festpreis&Projektart=Kundenprojekt%20Werkvertrag%20TimeandMaterial&Expanded=True&ShowArbeit=True&ShowUmsatz=True&ShowTeamprovision=True&ShowKunde=True&ShowRawData=True&rs%3AParameterLanguage=&rs%3ACommand=Render&rs%3AFormat=XML&rc%3AItemPath=Tablix1.Jahre.Monate.Tage";
        private static HttpWebRequest _http;
        static readonly HttpClient _client = new HttpClient();
        private static XmlNamespaceManager _namespaceManager = new XmlNamespaceManager(new NameTable());

        [FunctionName("ProvisionReportDownload")]
        public static async Task Run(
            [BlobTrigger("flowfiles/{name}")]Stream myBlob, string name, TraceWriter log,
            [EventHub("floweventhubinstance", Connection = "EventhubConnection")] IAsyncCollector<EventData> outputQueue)
        {
            _namespaceManager.AddNamespace("dft", "http://www.w3.org/2001/XMLSchema-instance");

            string usersPwd = await GetPwdFromKeyVault();

            int backwardsMonthCount = 0;
            var currentTime = DateTime.UtcNow.AddMonths(backwardsMonthCount);

            var uri = new Uri(string.Format(_uriTemplate, (currentTime.Month), "01", currentTime.Year,
                DateTime.DaysInMonth(currentTime.Year, (currentTime.Month))));
            _http = (HttpWebRequest)HttpWebRequest.Create(uri);

            CredentialCache credCache = SetCredentials(usersPwd);

            _http.Credentials = credCache;

            _http.Method = "GET";
            _http.KeepAlive = true;
            _http.Accept = "*/*";


            var response = _http.GetResponse();

            if (response.ContentLength == 0)
            {
                log.Error("Nothing to get");
            }
            else
            {
                using (var content = response.GetResponseStream())
                {
                    var reader = new StreamReader(content);
                    XDocument xml = HandleXml.GetTeamMemberProvisionDetails(reader);
                    try
                    {
                        var accountDetails = HandleXml.GetStrippedAccountDetails(xml);
                        var jsonData = JsonConvert.SerializeObject(new { AccountDetails = accountDetails, TimeStamp = currentTime });
                        var eventMessage = new EventData(System.Text.Encoding.Default.GetBytes(jsonData));
                        eventMessage.PartitionKey = "provision";
                        await outputQueue.AddAsync(eventMessage);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Sending to Q failed for provision xml", ex);
                    }
                }

            }
        }

        private static async Task<string> GetPwdFromKeyVault()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();

            var kvClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback), _client);

            string ttomowPwd = (await kvClient.GetSecretAsync($"{ConfigurationManager.AppSettings["KeyVaultUri"]}Secrets/alegricreds")).Value;
            return ttomowPwd;
        }

        private static CredentialCache SetCredentials(string ttomowPwd)
        {
            var credCache = new CredentialCache();
            credCache.Add(new Uri("https://pm.alegri.eu/"), "NTLM",
                new NetworkCredential(@"alegri\ttomow", ttomowPwd));
            return credCache;
        }
    }
}


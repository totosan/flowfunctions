using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FlowFunctionsTT;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowFunctionsTT
{
    public static class CockpitToCosmosDb
    {
        [FunctionName("CockpitToCosmosDb")]
        public static void Run([EventHubTrigger("floweventhubinstance",Connection = "EventhubConnection", ConsumerGroup = "second")] EventData eventMessage, TraceWriter log, 
          [DocumentDB("FlowDB", "CockpitCollection",CreateIfNotExists =true,ConnectionStringSetting = "CosmosDBConnection")] out dynamic document)
        {
            if (eventMessage.PartitionKey == "cockpit")
            {
                var doc = System.Text.Encoding.Default.GetString(eventMessage.GetBytes());
                document = doc;
            }
            else
            {
                document = null;
            }
        }
    }
}

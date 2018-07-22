using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace FlowFunctionsTT.Function.Provision
{
    public static class ProvisionToCosmosDB
    {
        [FunctionName("ProvisionToCosmosDB")]
        public static void Run([EventHubTrigger("floweventhubinstance", Connection = "EventhubConnection")] EventData myQueueItem, TraceWriter log,
            [DocumentDB("FlowDB", "ProvisionCollection", CreateIfNotExists = true, ConnectionStringSetting = "CosmosDBConnection")]
            out dynamic document)
        {
            if (myQueueItem.PartitionKey == "provision")
            {
                var json = System.Text.Encoding.Default.GetString(myQueueItem.GetBytes());
                document = json;
            }
            else
            {
                document = null;
            }
        }
    }
}

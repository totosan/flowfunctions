using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace FlowFunctionsTT.Function.Provision
{
    public static class ProvisionToCosmosDB
    {
        [FunctionName("ProvisionToCosmosDB")]
        public static void Run([QueueTrigger("ProjectServerDecomposedQ")]string myQueueItem, TraceWriter log, [DocumentDB("FlowDB", "ProvisionCollection", CreateIfNotExists = true, ConnectionStringSetting = "CosmosDBConnection")] out dynamic document)
        {
            document = myQueueItem;
        }
    }
}

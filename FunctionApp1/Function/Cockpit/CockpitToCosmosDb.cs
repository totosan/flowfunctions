using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FlowFunctionsTT;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowFunctionsTT
{
    public static class CockpitToCosmosDb
    {
  #if !DEBUG
      [FunctionName("CockpitToCosmosDb")]
        public static void Run([QueueTrigger("ExcelDecomposedQ")] string inputQueueItem, TraceWriter log, [DocumentDB("FlowDB", "CockpitCollection",CreateIfNotExists =true,ConnectionStringSetting = "CosmosDBConnection")] out dynamic document)
        {
            var unzipped = System.Text.Encoding.Default.GetBytes(inputQueueItem).Unzip();
            var output = (JObject)JsonConvert.DeserializeObject(unzipped);
            document = output;
        }
#endif
    }
}

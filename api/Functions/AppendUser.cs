using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NaftaScheduler
{
    public static class AppendUser
    {
        [FunctionName("AppendUser")]
        public static void Run([CosmosDBTrigger(
            databaseName: "Events",
            collectionName: "users",
            ConnectionStringSetting = "schedulercosmosdb_DOCUMENTDB",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists= true)]IReadOnlyList<Document> documents, ILogger log,
            ExecutionContext context)
        {
            Calendar calendar = new Calendar(context.FunctionAppDirectory);
            foreach (Document document in documents)
            {
                EventConfig e = JsonConvert.DeserializeObject<EventConfig>(document.ToString());
                log.LogCritical("readen event:");
                log.LogInformation(e.ToString());
            }
        }
    }
}

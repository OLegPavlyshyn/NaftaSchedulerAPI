using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NaftaScheduler
{
    public static class UsersController
    {
        [FunctionName("UsersController")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "Events",
            collectionName: "users",
            ConnectionStringSetting = "schedulercosmosdb_DOCUMENTDB",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists= true)]IReadOnlyList<Document> documents, ILogger log,
            ExecutionContext context)
        {
            Calendar calendar = new Calendar(context.FunctionAppDirectory);
            CosmosDBClient client = new CosmosDBClient("events");
            string query = @"SELECT * FROM c";
            List<EventConfig> classes = await client.QueryItemsAsync<EventConfig>(query);
            foreach (Document document in documents)
            {
                UserConfig user = JsonConvert.DeserializeObject<UserConfig>(document.ToString());
                foreach (var c in classes)
                {
                    calendar.AttendUser(c.gcEventId, user);
                    log.LogInformation($"User: {user.Name} added to event: {c.summary} at {c.startDate}.");
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NaftaScheduler;
using Newtonsoft.Json;

namespace Company.Function
{
    public static class EventController
    {
        [FunctionName("EventController")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "Events",
            collectionName: "events",
            ConnectionStringSetting = "schedulercosmosdb_DOCUMENTDB",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists= true)]IReadOnlyList<Document> documents, ILogger log,
            ExecutionContext context)
        {
            var users = new List<UserConfig>();
            users.Add(new UserConfig()
            {
                Name = "oleg.pawl@gmail.com",
                GroupId = "Kim-20-1(2)"
            });
            Calendar calendar = new Calendar(context.FunctionAppDirectory);
            log.LogInformation("Inneted calendar.");
            CosmosDBClient client = new CosmosDBClient("events");
            log.LogInformation("Inneted connection to Cosmos.");
            foreach (Document document in documents)
            {
                EventConfig e = JsonConvert.DeserializeObject<EventConfig>(document.ToString());
                if (e.toProcess)
                {
                    // create event 
                    if (e.gcEventId == null)
                    {
                        string eventId = calendar.CreateEvent(e, users);
                        e.gcEventId = eventId;
                        e.toProcess = false;
                        await client.UpdateItemAsync<EventConfig>(e.id, e);
                        log.LogInformation($"Added event: {e.summary} at {e.startDate} with id: {eventId}.");
                    }
                    // updateEvent
                    else if (e.IsActive)
                    {
                        calendar.UpdateEvent(e.gcEventId, e);
                        e.toProcess = false;
                        log.LogInformation($"Updated event: {e.summary} at {e.startDate} with id: {e.gcEventId}.");
                    }
                    // delete event
                    else
                    {
                        calendar.DeleteEvent(e.gcEventId);
                        await client.DeleteItemAsync<EventConfig>(e.id, e.startDate.ToString("s"));
                        log.LogInformation($"Delete event: {e.summary} at {e.startDate} with id: {e.gcEventId}.");
                    }

                }
            }
        }
    }
}

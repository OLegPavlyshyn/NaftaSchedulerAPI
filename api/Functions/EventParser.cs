using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NaftaScheduler
{
    public static class EventParser
    {
        [FunctionName("EventParser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            try
            {
                var client = new CosmosDBClient("events");
                log.LogInformation("Inneted connection to Cosmos.");
                string query = @"SELECT * FROM c WHERE c.startDate > GetCurrentDateTime()
                    ORDER BY c.startDate";
                var savedEventsList = await client.QueryItemsAsync<EventConfig>(query);
                log.LogInformation($"Readed: {savedEventsList.Count} saved events.");
                List<EventConfig> eventsList = await Parser.GetEvents();
                var parsedEventsList = eventsList.Where(e => e.startDate >= DateTime.Now).ToList();
                log.LogInformation($"Readed: {parsedEventsList.Count} parsed events.");
                foreach (var parsedEvent in parsedEventsList)
                {
                    var savedEvent = savedEventsList.Find(e => e.startDate == parsedEvent.startDate);
                    // add
                    if (savedEvent == null)
                    {
                        await client.AddItemAsync<EventConfig>(parsedEvent);
                        log.LogInformation($"Saved event: {parsedEvent.summary} at {parsedEvent.startDate}.");
                    }
                    else
                    {
                        // update
                        if (!EventConfig.Compare(parsedEvent, savedEvent))
                        {
                            parsedEvent.id = savedEvent.id;
                            await client.UpdateItemAsync<EventConfig>(parsedEvent.id, parsedEvent);
                            log.LogInformation($"Updated event: {parsedEvent.summary} at {parsedEvent.startDate}.");
                        }
                    }
                }

                DateTime[] parsedEventsDates = parsedEventsList.Select(e => e.startDate).ToArray();
                DateTime[] savedEventsDates = savedEventsList.Select(e => e.startDate).ToArray();
                foreach (DateTime date in savedEventsDates.Except(parsedEventsDates))
                {
                    // delete
                    var savedEvent = savedEventsList.Find(e => e.startDate == date);
                    savedEvent.IsActive = false;
                    savedEvent.toProcess = true;
                    await client.UpdateItemAsync<EventConfig>(savedEvent.id, savedEvent);
                    log.LogInformation($"Disabled event: {savedEvent.summary} at {savedEvent.startDate}.");
                }

                return new OkResult();
            }
            catch (System.Exception e)
            {
                log.LogError(e.ToString());
                throw e;
            }

        }
    }
}
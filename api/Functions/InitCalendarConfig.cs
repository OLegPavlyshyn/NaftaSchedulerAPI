using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NaftaScheduler
{
    public static class InitCalendarConfig
    {
        [FunctionName("InitCalendarConfigs")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            Calendar calendar = new Calendar(context.FunctionAppDirectory);

            return new OkObjectResult("Config saved.");
        }
    }
}

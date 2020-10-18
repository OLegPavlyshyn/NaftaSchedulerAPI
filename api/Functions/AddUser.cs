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
    public static class AddUser
    {
        [FunctionName("AddUser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            string[] emails = req.Query["users"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            emails = emails ?? data?.emails;
            CosmosDBClient client = new CosmosDBClient("users");
            if (emails.Length > 0)
            {
                var users = new UserConfig[emails.Length];
                for (int i = 0; i < emails.Length; i++)
                {
                    users[i] = new UserConfig()
                    {
                        Name = emails[i],
                        GroupId = "Kim-20-1" 
                    };
                    await client.AddItemAsync<UserConfig>(users[i]);
                    log.LogInformation($"User {users[i].Name} saved");
                }
            }
            return new OkResult();
        }
    }
}

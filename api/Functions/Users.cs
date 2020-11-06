using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace NaftaScheduler
{
    public static class Users
    {
        [FunctionName("Users")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "delete", Route = null)] HttpRequest req,
            ILogger log)
        {
            CosmosDBClient client = new CosmosDBClient("users");
            log.LogCritical(req.Method);
            if (req.Method == "GET")
            {
                var users = await client.QueryItemsAsync<UserConfig>();
                return new OkObjectResult(String.Join("\n", users.Select(u => u.ToString())));
            }
            else
            {
                string email = req.Query["email"];
                
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                email = email ?? data?.email;
                if (!String.IsNullOrEmpty(email))
                {
                    if (req.Method == "POST")
                    {
                        var user = new UserConfig()
                        {
                            Name = email,
                            groupID = "Kim-20-1"
                        };
                        try
                        {
                            var res = await client.ExecProcAsync<string>("addUser", user.groupID, new object[] { user });
                            return new OkObjectResult(res);
                        }
                        catch (System.Exception e)
                        {
                            log.LogError(e.Message);
                            return new BadRequestObjectResult(e.Message);
                        }
                    }
                    else if (req.Method == "DELETE")
                    {
                        try
                        {
                            var user = await client.QueryItemsAsync<UserConfig>($"SELECT * FROM c WHERE c.Name = '{email}'");
                            if (user.Count > 0)
                            {
                                await client.DeleteItemAsync<UserConfig>(user[0].id, user[0].groupID);
                                log.LogInformation($"User {user[0].Name} has been removed");
                                return new OkObjectResult($"User {user[0].Name} has been removed");
                            }
                            else
                            {
                                return new BadRequestObjectResult($"User with email: {email} does not exists");
                            }

                        }
                        catch (System.Exception e)
                        {
                            log.LogError(e.Message);
                            return new BadRequestObjectResult(e.Message);
                        }
                    }
                    else
                    {
                        return new BadRequestResult();
                    }
                }
                else
                {
                    return new BadRequestObjectResult("Please pass a valid email address");
                }
            }
        }
    }
}
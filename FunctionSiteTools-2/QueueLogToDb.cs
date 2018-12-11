using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionSiteTools
{
    public static class QueueLogToDb
    {
        [FunctionName("QueueLogToDb")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req,
           [Queue("logging")]out ActivityLogItem queuedLog,
           ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                queuedLog = JsonConvert.DeserializeObject<ActivityLogItem>(requestBody);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                queuedLog = null;
                return new BadRequestObjectResult(ex.Message);
            }

            return (ActionResult)new OkObjectResult("Message queued");
        }
    }
}

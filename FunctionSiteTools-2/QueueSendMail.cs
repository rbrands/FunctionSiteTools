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
    public static class QueueSendMail
    {
        /// <summary>
        /// Receives HTTP request with mail and push the mail to the queue to be send via SendGrid
        /// </summary>
        /// <param name="req"></param>
        /// <param name="queuedMail"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("QueueSendMail")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req,
            [Queue("sendmail")]out EMail queuedMail,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                queuedMail = JsonConvert.DeserializeObject<EMail>(requestBody);
                if (String.IsNullOrEmpty(queuedMail.Email))
                {
                    throw new ArgumentException("Receiver Email must not be empty.", "Email");
                }
                if (String.IsNullOrEmpty(queuedMail.From))
                {
                    throw new ArgumentException("From Email must not be empty.", "From");
                }
                if (String.IsNullOrEmpty(queuedMail.Subject))
                {
                    throw new ArgumentException("From Subject must not be empty.", "Subject");
                }
                if (String.IsNullOrEmpty(queuedMail.HtmlText))
                {
                    throw new ArgumentException("From HtmlText must not be empty.", "HtmlText");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                queuedMail = null;
                return new BadRequestObjectResult(ex.Message);
            }

            return (ActionResult)new OkObjectResult("Message queued");
        }
    }
}

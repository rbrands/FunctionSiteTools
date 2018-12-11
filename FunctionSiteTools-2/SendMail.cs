using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionSiteTools
{
    public static class SendMail
    {
        /// <summary>
        /// Sends a mail via SendGrid that is received from message queue "sendmail"
        /// </summary>
        /// <param name="myQueueItem"></param>
        /// <param name="message"></param>
        /// <param name="activityLog"></param>
        /// <param name="log"></param>
        [FunctionName("SendMail")]
        public static void Run([QueueTrigger("sendmail", Connection = "AzureWebJobsStorage")]string myQueueItem,
            [SendGrid(ApiKey = "SendGridApiKey")] out SendGridMessage message,
            [Queue("logging")]out ActivityLogItem activityLog,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
            EMail email = JsonConvert.DeserializeObject<EMail>(myQueueItem);
            message = new SendGridMessage();
            message.AddTo(email.Email);
            message.SetFrom(new EmailAddress(email.From, email.FromName));
            message.SetSubject(email.Subject);
            message.AddContent("text/html", email.HtmlText);
            log.LogInformation($"From: {email.From} - To: {email.Email} - Subject: {email.Subject}");
            activityLog = new ActivityLogItem();
            activityLog.User = email.From;
            activityLog.Category = "Info";
            activityLog.ClientInfo = "Functions.rbrandssitetools2.SendMail";
            activityLog.MessageTag = $"SendGrid to {email.Email}";
            activityLog.Message = email.Subject;
        }
    }

    public class EMail
    {
        [JsonProperty(PropertyName = "from", NullValueHandling = NullValueHandling.Ignore)]
        public string From { get; set; }
        [JsonProperty(PropertyName = "fromName", NullValueHandling = NullValueHandling.Ignore)]
        public string FromName { get; set; }
        [JsonProperty(PropertyName = "email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }
        [JsonProperty(PropertyName = "subject", NullValueHandling = NullValueHandling.Ignore)]
        public string Subject { get; set; }
        [JsonProperty(PropertyName = "htmlText", NullValueHandling = NullValueHandling.Ignore)]
        public string HtmlText { get; set; }
    }
}


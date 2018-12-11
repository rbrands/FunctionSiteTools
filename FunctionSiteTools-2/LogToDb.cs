using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;



namespace FunctionSiteTools
{
    public static class LogToDb
    {
        [FunctionName("LogToDb")]
        public static void Run([QueueTrigger("logging", Connection = "AzureWebJobsStorage")]string myQueueItem,
            [CosmosDB(
            databaseName: "rbrandssite",
            collectionName: "collection1",
            ConnectionStringSetting = "CosmosDBConnection")] out ActivityLogItem logItem,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
            logItem = JsonConvert.DeserializeObject<ActivityLogItem>(myQueueItem);
            if (String.IsNullOrEmpty(logItem.Tenant))
            {
                logItem.Tenant = "rbrands";
            }
            // logItem lives 2 days
            logItem.TimeToLive = 172800;
            string logItemSerialized = JsonConvert.SerializeObject(logItem);
            log.LogInformation($"LogToDb: {logItemSerialized}");
        }
    }
    public class ActivityLogItem : DocumentDBEntity
    {
        [JsonProperty(PropertyName = "category", NullValueHandling = NullValueHandling.Ignore)]
        public string Category { get; set; }
        [JsonProperty(PropertyName = "user", NullValueHandling = NullValueHandling.Ignore)]
        public string User { get; set; }
        [JsonProperty(PropertyName = "messageTag", NullValueHandling = NullValueHandling.Ignore)]
        public string MessageTag { get; set; }
        [JsonProperty(PropertyName = "message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
        [JsonProperty(PropertyName = "clientInfo", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientInfo { get; set; }

    }
    public class DocumentDBEntity : Resource
    {
        [JsonProperty(PropertyName = "tenant")]
        public string Tenant { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type
        {
            get
            {
                return this.GetType().Name;
            }
            set
            {

            }
        }
        // used to set expiration policy
        [JsonProperty(PropertyName = "ttl", NullValueHandling = NullValueHandling.Ignore)]
        public int? TimeToLive { get; set; }
    }

}

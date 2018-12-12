using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Services.AppAuthentication;
using Flurl.Http;

namespace FunctionSiteTools
{
    public static class InviteUser
    {
        [FunctionName("InviteUser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://graph.microsoft.com/");

            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                InvitedUser invitedUser = JsonConvert.DeserializeObject<InvitedUser>(requestBody);
                // Call Graph API
                dynamic response = await $"https://graph.microsoft.com/v1.0/invitations"
                               .WithOAuthBearerToken(accessToken)
                               .PostJsonAsync(invitedUser);

            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
            return (ActionResult)new OkObjectResult("User invited");

        }
    }
    public class InvitedUser
    {
        [JsonProperty(PropertyName = "invitedUserDisplayName", NullValueHandling = NullValueHandling.Ignore)]
        public string InvitedUserDisplayName { get; set; }
        [JsonProperty(PropertyName = "invitedUserEmailAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string InvitedUserEmailAddress { get; set; }
        [JsonProperty(PropertyName = "inviteRedirectUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string InviteRedirectUrl { get; set; }
    }
}

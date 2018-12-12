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
    public static class CreateUser
    {
        [FunctionName("CreateUser")]
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
                AzureAdUser newUser = JsonConvert.DeserializeObject<AzureAdUser>(requestBody);
                // Call Graph API
                dynamic response = await $"https://graph.microsoft.com/v1.0/users"
                               .WithOAuthBearerToken(accessToken)
                               .PostJsonAsync(newUser);
                return new JsonResult(response.value);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
    public class AzureAdUser
    {
        [JsonProperty(PropertyName = "accountEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public Boolean AccountEnabled { get; set; }
        [JsonProperty(PropertyName = "displayName", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }
        [JsonProperty(PropertyName = "mailNickname", NullValueHandling = NullValueHandling.Ignore)]
        public string MailNickname { get; set; }
        [JsonProperty(PropertyName = "userPrincipalName", NullValueHandling = NullValueHandling.Ignore)]
        public string UserPrincipalName { get; set; }
        [JsonProperty(PropertyName = "passwordProfile", NullValueHandling = NullValueHandling.Ignore)]
        public PasswordProfile PasswordProfile { get; set; }
    }
    public class PasswordProfile
    {
        public Boolean forceChangePasswordNextSignIn { get; set; }
        public string password { get; set; }
    }

}


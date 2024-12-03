using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Services.AppAuthentication;
using Flurl.Http;
using Microsoft.Azure.Functions.Worker;

namespace FunctionSiteTools
{
    public class InviteUser
    {
        readonly ILogger _logger;
        public InviteUser(ILogger logger) 
        { 
            _logger = logger;
        }
        [Function("InviteUser")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://graph.microsoft.com/");

            try
            {
                string requestBody;
                using (var reader = new StreamReader(req.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                InvitedUser invitedUser = JsonConvert.DeserializeObject<InvitedUser>(requestBody);
                // Call Graph API
                var response = await $"https://graph.microsoft.com/v1.0/invitations"
                               .WithOAuthBearerToken(accessToken)
                               .PostJsonAsync(invitedUser)
                               .ReceiveJson<GraphResponse>();
                return new JsonResult(response);
            }
            catch (FlurlHttpException ex)
            {
                var errorResponse = await ex.GetResponseStringAsync();
                _logger.LogError($"Request failed: {ex.Message}, Response: {errorResponse}");
                return new BadRequestObjectResult($"Request failed: {ex.Message}, Response: {errorResponse}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }

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

    public class GraphResponse
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
        [JsonProperty(PropertyName = "invitedUserDisplayName")]
        public string InvitedUserDisplayName { get; set; }
        [JsonProperty(PropertyName = "invitedUserEmailAddress")]
        public string InvitedUserEmailAddress { get; set; }
    }
}

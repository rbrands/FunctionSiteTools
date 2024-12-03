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
    public class CreateUser
    {
        private readonly ILogger _logger;

        public CreateUser(ILogger<CreateUser> logger)
        {
            _logger = logger;
        }

        [Function("CreateUser")]
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
                AzureAdUser newUser = JsonConvert.DeserializeObject<AzureAdUser>(requestBody);
                // Call Graph API
                var response = await $"https://graph.microsoft.com/v1.0/users"
                               .WithOAuthBearerToken(accessToken)
                               .PostJsonAsync(newUser)
                               .ReceiveJson<AzureAdUser>();
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


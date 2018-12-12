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
    public static class ReadGroups
    {
        /// <summary>
        /// Returns all groups the user given as query argument "upn" is member of.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("ReadGroups")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger ReadGroups function processed a request.");
            // Check if query argument upn is provided
            string userPrincipalName = req.Query["upn"];
            if (String.IsNullOrEmpty(userPrincipalName))
            {
                return new BadRequestObjectResult("Please pass an upn on the query string");
            }
            // Get access token for service principal
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://graph.microsoft.com/");
            try
            {
                // Call Graph API
                dynamic response = await $"https://graph.microsoft.com/v1.0/users/{userPrincipalName}/memberOf"
                               .WithOAuthBearerToken(accessToken)
                               .GetJsonAsync();
                
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}

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
    public static class ReadGroupMembers
    {
        /// <summary>
        /// Gets all memebers of given group. 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("ReadGroupMembers")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger ReadGroupMembers function processed a request.");

            string groupId = req.Query["id"];
            string displayName = req.Query["displayName"];
            if (String.IsNullOrEmpty(groupId) && String.IsNullOrEmpty(displayName))
            {
                return new BadRequestObjectResult("Please pass an group id or displayName on the query string");
            }
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://graph.microsoft.com/");
            if (String.IsNullOrEmpty(groupId))
            {
                try
                {
                    // Get group id from displayName
                    dynamic groupResponse = await $"https://graph.microsoft.com/v1.0/groups/?$filter=displayName eq '{displayName}'"
                                        .WithOAuthBearerToken(accessToken)
                                        .GetJsonAsync();
                    var groups = groupResponse.value;
                    if (groups.Count == 0)
                    {
                        throw new Exception("No group found with displayName");
                    }
                    groupId = groups[0].id;
                }
                catch (Exception ex)
                {
                    log.LogError(ex.Message);
                    return new BadRequestObjectResult(ex.Message);
                }
            }

            try
            {
                dynamic response = await $"https://graph.microsoft.com/v1.0/groups/{groupId}/members"
                               .WithOAuthBearerToken(accessToken)
                               .GetJsonAsync();
                var members = response.value;
                return new JsonResult(members);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

using Microsoft.Azure.Services.AppAuthentication;
using Flurl.Http;
using Microsoft.Azure.Functions.Worker;

namespace FunctionSiteTools
{
    public class ReadGroups
    {
        readonly ILogger<ReadGroups> _logger;
        public ReadGroups(ILogger<ReadGroups> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns all groups the user given as query argument "upn" is member of.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [Function("ReadGroups")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger ReadGroups function processed a request.");
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
                var response = await $"https://graph.microsoft.com/v1.0/users/{userPrincipalName}/memberOf"
                               .WithOAuthBearerToken(accessToken)
                               .GetJsonAsync<GraphGroupsResponse>();
                var groups = response.Value;
                return new JsonResult(groups);
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

    public class GraphGroupsResponse
    {
        [JsonProperty("value")]
        public List<Group> Value { get; set; }
    }

    public class Group
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }
}

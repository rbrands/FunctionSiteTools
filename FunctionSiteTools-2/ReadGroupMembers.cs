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
    public class ReadGroupMembers
    {
        private readonly ILogger _logger;
        public ReadGroupMembers(ILogger<TranslateText> logger)
        {
            _logger = logger;
        }
        public class Group
        {
            public string id { get; set; }
        }

        public class Member
        {
            public string id { get; set; }
            public string displayName { get; set; }
        }

        public class GroupResponse
        {
            public List<Group> value { get; set; }
        }

        public class MemberResponse
        {
            public List<Member> value { get; set; }
        }

        /// <summary>
        /// Gets all members of given group. 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [Function("ReadGroupMembers")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger ReadGroupMembers function processed a request.");

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
                    var groupResponse = await $"https://graph.microsoft.com/v1.0/groups/?$filter=displayName eq '{displayName}'"
                                        .WithOAuthBearerToken(accessToken)
                                        .GetJsonAsync<GroupResponse>();
                    var groups = groupResponse.value;
                    if (groups.Count == 0)
                    {
                        throw new Exception("No group found with displayName");
                    }
                    groupId = groups[0].id;
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

            try
            {
                var response = await $"https://graph.microsoft.com/v1.0/groups/{groupId}/members"
                               .WithOAuthBearerToken(accessToken)
                               .GetJsonAsync<MemberResponse>();
                var members = response.value;
                return new JsonResult(members);
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
}

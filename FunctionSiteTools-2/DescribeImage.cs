using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Flurl.Http;


namespace FunctionSiteTools
{
    public static class DescribeImage
    {
        [FunctionName("DescribeImage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger DescribeImage processed a request.");
            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                var image = JsonConvert.DeserializeObject(requestBody);
                // Call CognitiveServices
                dynamic response = await "https://westeurope.api.cognitive.microsoft.com/vision/v2.0/describe"
                                   .WithHeader("Ocp-Apim-Subscription-Key", TranslateText.GetEnvironmentVariable("ComputerVisionApiKey"))
                                   .PostJsonAsync(image)
                                   .ReceiveJson();
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

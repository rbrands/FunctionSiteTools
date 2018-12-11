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
    public static class TranslateText
    {
        [FunctionName("TranslateText")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string to = req.Query["to"];
            if (String.IsNullOrEmpty(to))
            {
                return new BadRequestObjectResult("Please pass targetLanguage to on the query string");
            }

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string textToTranslate = data?.text;
            if (String.IsNullOrEmpty(textToTranslate))
            {
                return new BadRequestObjectResult("Please pass text to translate in request.");
            }
            string translatorApiKey = GetEnvironmentVariable("TranslatorApiKey");
            if (String.IsNullOrEmpty(translatorApiKey))
            {
                return new BadRequestObjectResult("Please configure AppSetting TranslatorApiKey with key for Cognitive Services.");
            }
            try
            {
                System.Object[] body = new System.Object[] { new { Text = textToTranslate } };

                var response = await $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0"
                               .WithHeader("Ocp-Apim-Subscription-Key", translatorApiKey)
                               .SetQueryParam("to", to)
                               .SetQueryParam("textType", "html")
                               .SetQueryParam("from", "de")
                               .PostJsonAsync(body)
                               .ReceiveJsonList();
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
        public static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Flurl.Http;
using Microsoft.Azure.Functions.Worker;

namespace FunctionSiteTools
{
    public class TranslateText
    {
        private readonly ILogger _logger;
        public TranslateText(ILogger<TranslateText> logger)
        {
            _logger = logger;
        }

        [Function("TranslateText")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("TranslateText");

            string to = req.Query["to"];
            if (String.IsNullOrEmpty(to))
            {
                return new BadRequestObjectResult("Please pass targetLanguage to on the query string");
            }

            try
            {
                string requestBody;
                using (var reader = new StreamReader(req.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                var data = JsonConvert.DeserializeObject<TranslateRequest>(requestBody);
                if (data == null || String.IsNullOrEmpty(data.Text))
                {
                    return new BadRequestObjectResult("Please pass text to translate in request.");
                }
                string textToTranslate = data.Text;

                string translatorApiKey = GetEnvironmentVariable("TranslatorApiKey");
                if (String.IsNullOrEmpty(translatorApiKey))
                {
                    return new BadRequestObjectResult("Please configure AppSetting TranslatorApiKey with key for Cognitive Services.");
                }
                var body = new[] { new { Text = textToTranslate } };

                var response = await $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0"
                                .WithHeader("Ocp-Apim-Subscription-Key", translatorApiKey)
                                .SetQueryParam("to", to)
                                .SetQueryParam("textType", "html")
                                .SetQueryParam("from", "de")
                                .PostJsonAsync(body)
                                .ReceiveJson<List<TranslationResponse>>();
                _logger.LogInformation(response.ToString());
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

        public static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        public class TranslateRequest
        {
            public string Text { get; set; }
        }

        public class TranslationResponse
        {
            public List<Translation> Translations { get; set; }
        }

        public class Translation
        {
            public string Text { get; set; }
            public string To { get; set; }
        }
    }
}

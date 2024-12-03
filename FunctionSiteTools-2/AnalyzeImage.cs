using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.Functions.Worker;

namespace FunctionSiteTools
{
    public class AnalyzeImage
    {
        private readonly ILogger _logger;
        public AnalyzeImage(ILogger<TranslateText> logger)
        {
            _logger = logger;
        }

        [Function("AnalyzeImage")]
        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger AnalyzeImage processed a request.");
            try
            {
                string requestBody;
                using (var reader = new StreamReader(req.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                var image = JsonConvert.DeserializeObject<ImageRequest>(requestBody);
                if (image == null || string.IsNullOrEmpty(image.Url))
                {
                    return new BadRequestObjectResult("Please pass a valid image URL in the request body.");
                }
                _logger.LogInformation("Image URL: " + image.Url);
                // Call CognitiveServices
                string computerVisionKey = TranslateText.GetEnvironmentVariable("ComputerVisionApiKey");
                var response = await "https://westeurope.api.cognitive.microsoft.com/vision/v2.0/analyze"
                                   .SetQueryParams(new {
                                        visualFeatures = "Categories,Tags,Description,Faces,ImageType,Color,Adult",
                                        details = "Landmarks"
                                   })
                                   .WithHeader("Ocp-Apim-Subscription-Key", computerVisionKey)
                                   .PostJsonAsync(image)
                                   .ReceiveJson<ImageAnalysisResult>();
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

    public class ImageAnalysisResult
    {
        public Description Description { get; set; }
        public string RequestId { get; set; }
        public Metadata Metadata { get; set; }
    }
}

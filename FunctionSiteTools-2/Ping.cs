using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;

namespace FunctionSiteTools
{
    public class Ping
    {
        private readonly ILogger _logger;
        public Ping(ILogger<Ping> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// Sample function that works as "Ping" of Azure Functions.
        /// A argument "name" can be part of query string or part of the body and will be shown as response.
        /// Use function key to call.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [Function("Ping")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed 'Ping''.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name} Version 2024-12-03")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}

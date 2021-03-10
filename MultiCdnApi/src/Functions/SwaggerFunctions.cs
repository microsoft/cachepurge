// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MultiCdnApi
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureFunctions.Extensions.Swashbuckle;
    using AzureFunctions.Extensions.Swashbuckle.Attribute;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;

    public static class SwaggerFunctions
    {
        [SwaggerIgnore]
        [FunctionName("SwaggerJson")]
        public static Task<HttpResponseMessage> SwaggerJson(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "swagger/json")]
            HttpRequestMessage requestMessage,
            HttpRequest request,
            [SwashBuckleClient] ISwashBuckleClient swashBuckleClient,
            ILogger log)
        {
            log.LogInformation($"{nameof(SwaggerJson)}; " +
                               $"invoked by {request.HttpContext.User?.Identity?.Name}");
            return Task.FromResult(swashBuckleClient.CreateSwaggerDocumentResponse(requestMessage));
        }

        [SwaggerIgnore]
        [FunctionName("SwaggerUi")]
        public static Task<HttpResponseMessage> SwaggerUi(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "swagger/ui")]
            HttpRequestMessage requestMessage,
            HttpRequest req,
            [SwashBuckleClient] ISwashBuckleClient swashBuckleClient,
            ILogger log)
        {
            log.LogInformation($"{nameof(SwaggerUi)}; " +
                               $"invoked by {req.HttpContext.User?.Identity?.Name}");
            return Task.FromResult(swashBuckleClient.CreateSwaggerUIResponse(requestMessage, "swagger/json"));
        }
    }
}
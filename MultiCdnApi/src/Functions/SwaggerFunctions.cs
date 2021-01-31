// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MultiCdnApi
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzureFunctions.Extensions.Swashbuckle;
    using AzureFunctions.Extensions.Swashbuckle.Attribute;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;

    public static class SwaggerFunctions
    {
        [SwaggerIgnore]
        [FunctionName("SwaggerJson")]
        public static Task<HttpResponseMessage> SwaggerJson(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "swagger/json")]
            HttpRequestMessage req,
            [SwashBuckleClient] ISwashBuckleClient swashBuckleClient)
        {
            return Task.FromResult(swashBuckleClient.CreateSwaggerDocumentResponse(req));
        }

        [SwaggerIgnore]
        [FunctionName("SwaggerUi")]
        public static Task<HttpResponseMessage> SwaggerUi(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "swagger/ui")]
            HttpRequestMessage req,
            [SwashBuckleClient] ISwashBuckleClient swashBuckleClient)
        {
            return Task.FromResult(swashBuckleClient.CreateSwaggerUIResponse(req, "swagger/json"));
        }
    }
}
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MultiCdnApi
{
    using System.Linq;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;

    public class UserFunctions
    {
        [FunctionName("GetUserHeaders")]
        public IActionResult GetUserHeaders(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/headers")]
            HttpRequest req,
            ILogger log)
        {
            var headerDictionary = req.Headers;
            return new JsonResult(headerDictionary);
        }

        [FunctionName("GetUser")]
        public IActionResult GetUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user")]
            HttpRequest req,
            ILogger log)
        {
            var httpContext = req.HttpContext;
            var httpContextUser = httpContext.User;
            return new JsonResult(httpContextUser);
        }

        [FunctionName("GetGroups")]
        public IActionResult GetGroups(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/groups")]
            HttpRequest req,
            ILogger log)
        {
            var httpContext = req.HttpContext;
            var httpContextUser = httpContext.User;
            return new JsonResult(
                string.Join("; ",
                    httpContextUser.Claims.Where(claim => claim.Type == "groups")
                        .Select(c => c.Type + " : " + c.Value)));
        }
    }
}
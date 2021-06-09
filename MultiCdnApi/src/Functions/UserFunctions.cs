// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MultiCdnApi
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Identity.Web.Resource;

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
            var result = new StringBuilder();

            result.Append("Claims: {\n");
            foreach (var claim in httpContextUser.Claims)
            {
                result.Append("claim: {").Append(SerializeClaim(claim)).Append("};\n");
            }
            result.Append("}\n");

            result.Append("Identities: {\n");
            foreach (var claimsIdentity in httpContextUser.Identities)
            {
                result.Append("identity: {");
                result.Append(string.Join(",", claimsIdentity.Claims)).Append(";");
                result.Append(string.Join(",",
                    claimsIdentity.Label, claimsIdentity.Name, claimsIdentity.AuthenticationType,
                    claimsIdentity.NameClaimType, claimsIdentity.RoleClaimType)).Append(";");
                result.Append("};\n");
            }
            result.Append("}\n");

            result.Append("Identity: {\n");
            result.Append(httpContextUser.Identity);
            result.Append("}\n");

            return new JsonResult(result.ToString());
        }

        [FunctionName("GetClaims")]
        public IActionResult GetClaims(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/claims")]
            HttpRequest req,
            ILogger log)
        {
            var httpContext = req.HttpContext;
            var httpContextUser = httpContext.User;
            return new JsonResult(string.Join("; ", httpContextUser.Claims.Select(SerializeClaim)));
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

        [FunctionName("GetRoles")]
        public IActionResult GetRoles(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/roles")]
            HttpRequest req,
            ILogger log)
        {
            var roleClaim = ClaimsPrincipal.Current.FindFirst("roles");
            return new JsonResult(roleClaim);
        }

        [FunctionName("GetApiScopes")]
        public IActionResult GetApiScopes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/scopes")]
            HttpRequest req,
            ILogger log)
        {
            req.HttpContext.ValidateAppRole("");
            return new JsonResult("OK");
        }

        private static string SerializeClaim(Claim claim)
        {
            var claimProperties = string.Join(";",
                claim.Issuer, claim.OriginalIssuer, Serialize(claim.Properties), Serialize(claim.Subject), claim.ValueType);
            return claim + " (" + claimProperties + ")";
        }

        private static string Serialize(ClaimsIdentity cI)
        {
            return string.Join(",", cI.Name, cI.Label, cI.AuthenticationType, cI.NameClaimType, cI.RoleClaimType);
        }

        private static string Serialize<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            return "{" + string.Join(";", dict.Select(kv => kv.Key + "->" + kv.Value)) + "}";
        }
    }
}
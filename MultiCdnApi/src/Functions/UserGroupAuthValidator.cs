// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MultiCdnApi
{
    using CdnLibrary;
    using Microsoft.AspNetCore.Http;
    using Azure.Identity;

    /// <summary>
    /// Note: can potentially be replaced by Azure Function Filters -
    /// https://github.com/Azure/azure-webjobs-sdk/wiki/Function-Filters -
    /// when they become a production feature.
    /// </summary>
    public static class UserGroupAuthValidator
    {
        public static void CheckUserAuthorized(HttpRequest req)
        {
            if (!IsUserAuthorized(req))
            {
                throw new AuthenticationFailedException("User is not authorized");
            }
        }

        private static bool IsUserAuthorized(HttpRequest req)
        {
            if (!EnvironmentConfig.AuthorizationEnabled) {
                return true;
            }

            var roleClaims = req.HttpContext.User.FindAll("roles");
            foreach (var roleClaim in roleClaims)
            {
                if (EnvironmentConfig.AuthorizedGroup.Equals(roleClaim.Value))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnPlugin
{
    using CdnLibrary;
    using System;
    using System.Collections.Generic;

    public static class TestEnvironmentConfig
    {
        private static readonly Dictionary<string, string> EnvironmentVariables = new Dictionary<string, string>
        {
            [EnvironmentConfig.AuthorizationEnabledEnvironmentVariable] = "False", // do not use group auth for staging and local environments
            [EnvironmentConfig.CosmosDBConnection] = "CosmosDBConnection",
            [EnvironmentConfig.CosmosDatabaseId] = "CacheOut",
            [EnvironmentConfig.PartnerCosmosContainerId] = "partners",
            [EnvironmentConfig.UserRequestCosmosContainerId] = "userRequests",
        };

        public static void SetupTestEnvironment()
        {
            foreach (var (envKey, envValue) in EnvironmentVariables)
            {
                Environment.SetEnvironmentVariable(envKey, envValue);
            }
        }
    }
}
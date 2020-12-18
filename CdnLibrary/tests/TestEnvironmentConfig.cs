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
        public static readonly Dictionary<string, string> EnvironmentVariables = new Dictionary<string, string>
        {
            [EnvironmentConfig.CosmosDBConnection] = "CosmosDBConnection", // 'CacheOutTestCosmosDbAuthKey' - for 'cacheouttestdb' Cosmos DB 
            [EnvironmentConfig.CosmosDatabaseId] = "CacheOut", // 'CacheOut' - for 'cacheouttestdb' Cosmos DB
            
            // 'userRequestToPartnerRequest' - for 'cacheouttestdb' Cosmos DB, for testing only
            // 'AFD_PartnerRequest' - for 'cacheouttestdb' Cosmos DB, for AFD testing
            // todo: 'Akamai_CdnRequest' - for 'cacheouttestdb' Cosmos DB, for Akamai testing
            [EnvironmentConfig.PartnerCosmosContainerId] = "partners", // 'partners' - for 'cacheouttestdb' Cosmos DB
            [EnvironmentConfig.UserRequestCosmosContainerId] = "userRequests", // 'userRequests' - for 'cacheouttestdb' Cosmos DB
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
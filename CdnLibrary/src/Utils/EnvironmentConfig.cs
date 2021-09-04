/* -----------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 * ----------------------------------------------------------------------- */

namespace CdnLibrary
{
    using System;

    /// <summary>
    /// See tests for sample values
    /// </summary>
    public static class EnvironmentConfig
    {
        internal const string DatabaseName = "CacheOut";
        internal const string CosmosDBConnectionStringName = "CosmosDBConnection";
        internal const string BatchQueueConnectionStringName = "CDN_Queue";

        internal const string AfdPartnerCollectionName = "AFD_PartnerRequest";
        internal const string AfdCdnCollectionName = "AFD_CdnRequest";
        internal const string AfdBatchQueueName = "AFDBatchQueue";

        internal const string AkamaiPartnerCollectionName = "Akamai_PartnerRequest";
        internal const string AkamaiCdnCollectionName = "Akamai_CdnRequest";
        internal const string AkamaiBatchQueueName = "AkamaiBatchQueue";

        internal const string CosmosDBConnection = "CosmosDBConnection";

        public static string CosmosDBConnectionString => Environment.GetEnvironmentVariable(CosmosDBConnection) ?? throw new InvalidOperationException();

        public static string CosmosDatabaseId => Environment.GetEnvironmentVariable("CosmosDatabaseId") ?? "CacheOut";
        
        public static string UserRequestCosmosContainerId => Environment.GetEnvironmentVariable("UserRequestCosmosContainerId") ?? "UserRequest";

        public static string PartnerCosmosContainerId => Environment.GetEnvironmentVariable("PartnerCosmosContainerId") ?? "Partner";

        public static int MaxRetry => (Environment.GetEnvironmentVariable("Max_Retry") != null) ? Convert.ToInt32(Environment.GetEnvironmentVariable("Max_Retry")) : 5;

        public static string CdnRequestTablePartitionKey => Environment.GetEnvironmentVariable("CdnRequestTablePartitionKey") ?? "PartnerRequestID";

        public static string PartnerRequestTablePartitionKey => Environment.GetEnvironmentVariable("PartnerRequestTablePartitionKey") ?? "UserRequestID";

        public static string UserRequestTablePartitionKey => Environment.GetEnvironmentVariable("UserRequestTablePartitionKey") ?? "id";

        public static int RequestWaitTime = (Environment.GetEnvironmentVariable("Poll_WaitTime") != null) ? Convert.ToInt32(Environment.GetEnvironmentVariable("Poll_WaitTime")) : 2;

        public static int AfdBatchSize = (Environment.GetEnvironmentVariable("Afd_UrlBatchSize")) != null ? Convert.ToInt32(Environment.GetEnvironmentVariable("Afd_UrlBatchSize")) : 200;

        public static int AkamaiBatchSize = (Environment.GetEnvironmentVariable("Akamai_UrlBatchSize")) != null ? Convert.ToInt32(Environment.GetEnvironmentVariable("Akamai_UrlBatchSize")) : 200;

        public static bool AuthorizationEnabled =
            Environment.GetEnvironmentVariable("AuthorizationEnabled") != null
                ? bool.Parse(Environment.GetEnvironmentVariable("AuthorizationEnabled"))
                : true;

        
        public static readonly string AuthorizedGroup = Environment.GetEnvironmentVariable("AuthorizedGroup");
        public static readonly string AzureFunctionsAccessKey = Environment.GetEnvironmentVariable("AzureFunctionsAccessKey");
    }
}